using DocumentProcessor.Contracts.Commands;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Worker.Interfaces;
using MassTransit;
using DocumentProcessor.Contracts;

namespace DocumentProcessor.Worker.Consumers;

public class ProcessFileCommandConsumer : IConsumer<ProcessFileCommand>
{
    private readonly ILogger<ProcessFileCommandConsumer> _logger;
    private readonly IFileReader _fileReader;
    private readonly ITextProcessor _textProcessor;

    public ProcessFileCommandConsumer(
        ILogger<ProcessFileCommandConsumer> logger,
        IFileReader fileReader,
        ITextProcessor textProcessor)
    {
        _logger = logger;
        _fileReader = fileReader;
        _textProcessor = textProcessor;
    }

            public async Task Consume(ConsumeContext<ProcessFileCommand> context)
        {
            var processId = context.Message.ProcessId;
            var fileId = context.Message.FileId;
            var fileName = context.Message.FileName;

            _logger.LogInformation("WORKER: Starting file processing - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}", 
                processId, fileId, fileName);

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("File name cannot be null or empty");
                }

                if (processId == Guid.Empty)
                {
                    throw new ArgumentException("Process ID cannot be empty");
                }

                await context.Publish(new UpdateProcessStatusCommand
                {
                    ProcessId = processId,
                    NewStatus = ProcessStatus.Running,
                    Reason = "First file processing started or file processing in progress."
                });

                var filePath = Path.Combine("/app/documents", processId.ToString(), fileName);
                
                var content = await _fileReader.ReadFileAsync(filePath);
                var result = await _textProcessor.ProcessTextAsync(content);
                
                _logger.LogInformation("WORKER: File analysis complete - Words: {WordCount}, Lines: {LineCount}, Characters: {CharCount}", 
                    result.WordCount, result.LineCount, result.CharacterCount);

                await context.Publish(new FileProcessedEvent
                {
                    ProcessId = processId,
                    FileId = fileId,
                    FileName = fileName,
                    WordCount = result.WordCount,
                    LineCount = result.LineCount,
                    CharacterCount = result.CharacterCount,
                    MostFrequentWords = result.MostFrequentWordsArray,
                    Summary = result.Summary
                });

                _logger.LogInformation("WORKER: Successfully processed file {FileName} - Publishing FileProcessedEvent", fileName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "WORKER: Invalid input processing file {FileName}", fileName);
                await context.Publish(new FileFailedEvent
                {
                    ProcessId = processId,
                    FileId = fileId,
                    FileName = fileName,
                    ErrorMessage = $"Invalid input: {ex.Message}"
                });
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "WORKER: File not found {FileName}", fileName);
                await context.Publish(new FileFailedEvent
                {
                    ProcessId = processId,
                    FileId = fileId,
                    FileName = fileName,
                    ErrorMessage = $"File not found: {ex.Message}"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "WORKER: Processing error for file {FileName}", fileName);
                await context.Publish(new FileFailedEvent
                {
                    ProcessId = processId,
                    FileId = fileId,
                    FileName = fileName,
                    ErrorMessage = $"Processing error: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WORKER: Unexpected error processing file {FileName}", fileName);
                await context.Publish(new FileFailedEvent
                {
                    ProcessId = processId,
                    FileId = fileId,
                    FileName = fileName,
                    ErrorMessage = $"Unexpected error: {ex.Message}"
                });
            }
        }
} 