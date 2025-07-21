using MassTransit;
using DocumentProcessor.Contracts.Commands;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Dao.Interfaces;
using DocumentProcessor.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Writer.Consumers;

public class InitializeFilesCommandConsumer : IConsumer<InitializeFilesCommand>
{
    private readonly IWriterRepository _writerRepository;
    private readonly ILogger<InitializeFilesCommandConsumer> _logger;

    public InitializeFilesCommandConsumer(
        IWriterRepository writerRepository,
        ILogger<InitializeFilesCommandConsumer> logger)
    {
        _writerRepository = writerRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InitializeFilesCommand> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("WRITER: Initializing files for process - ProcessId: {ProcessId}, FileCount: {FileCount}", 
            message.ProcessId, message.FileNames.Count);

        try
        {
            var files = new List<FileDTO>();
            
            foreach (var fileName in message.FileNames)
            {
                var file = new FileDTO
                {
                    FileId = Guid.NewGuid(),
                    ProcessId = message.ProcessId,
                    FileName = fileName,
                    Status = "PENDING",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                files.Add(file);
            }

            await _writerRepository.CreateFilesAsync(files);
            
            _logger.LogInformation("WRITER: Files initialized successfully - ProcessId: {ProcessId}, FileCount: {FileCount}", 
                message.ProcessId, files.Count);

            foreach (var file in files)
            {
                await context.Publish(new FileReadyEvent
                {
                    ProcessId = file.ProcessId,
                    FileId = file.FileId,
                    FileName = file.FileName
                });
                
                _logger.LogInformation("WRITER: Published FileReadyEvent for {FileName}", file.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WRITER: Failed to initialize files - ProcessId: {ProcessId}", message.ProcessId);
            throw;
        }
    }
} 