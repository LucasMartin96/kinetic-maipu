using MassTransit;
using DocumentProcessor.Contracts.Commands;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Dao.Interfaces;

namespace DocumentProcessor.Writer.Consumers;

public class PersistFileResultCommandConsumer : IConsumer<PersistFileResultCommand>
{
    private readonly IWriterRepository _writerRepository;
    private readonly ILogger<PersistFileResultCommandConsumer> _logger;

    public PersistFileResultCommandConsumer(
        IWriterRepository writerRepository,
        ILogger<PersistFileResultCommandConsumer> logger)
    {
        _writerRepository = writerRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PersistFileResultCommand> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("WRITER: Persisting file result - ProcessId: {ProcessId}, FileId: {FileId}, Status: {Status}", 
            message.ProcessId, message.FileId, message.Status);

        try
        {
            var file = await _writerRepository.GetFileByIdAsync(message.FileId);
            if (file == null)
            {
                _logger.LogWarning("File not found: FileId={FileId}", message.FileId);
                return;
            }

            file.Status = message.Status;
            file.UpdatedAt = DateTime.UtcNow;

            // TODO: implementar un enum 
            if (message.Status == "COMPLETED")
            {
                file.WordCount = message.WordCount;
                file.LineCount = message.LineCount;
                file.CharacterCount = message.CharacterCount;
                file.MostFrequentWords = string.Join(", ", message.MostFrequentWords);
                file.Summary = message.Summary;
            }
            else if (message.Status == "FAILED")
            {
                file.ErrorMessage = message.ErrorMessage;
            }

            await _writerRepository.UpdateFileAsync(file);
            
            await context.Publish(new FilePersistedEvent
            {
                ProcessId = message.ProcessId,
                FileId = message.FileId,
                FileName = message.FileName,
                Status = message.Status
            });
            
            _logger.LogInformation("WRITER: File result persisted successfully - FileId: {FileId}, Status: {Status}", 
                message.FileId, message.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist file result: FileId={FileId}, Status={Status}", 
                message.FileId, message.Status);
            throw;
        }
    }
} 