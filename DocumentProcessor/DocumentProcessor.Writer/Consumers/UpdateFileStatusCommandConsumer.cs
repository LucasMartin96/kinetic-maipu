using MassTransit;
using DocumentProcessor.Contracts.Commands;
using DocumentProcessor.Dao.Interfaces;
using DocumentProcessor.Contracts.Dtos;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Writer.Consumers;

public class UpdateFileStatusCommandConsumer : IConsumer<UpdateFileStatusCommand>
{
    private readonly IWriterRepository _writerRepository;
    private readonly ILogger<UpdateFileStatusCommandConsumer> _logger;

    public UpdateFileStatusCommandConsumer(
        IWriterRepository writerRepository,
        ILogger<UpdateFileStatusCommandConsumer> logger)
    {
        _writerRepository = writerRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateFileStatusCommand> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Updating file status: ProcessId={ProcessId}, FileId={FileId}, NewStatus={NewStatus}", 
            message.ProcessId, message.FileId, message.NewStatus);

        try
        {
            var file = await _writerRepository.GetFileByIdAsync(message.FileId);


            if (file == null)
            {
                _logger.LogWarning("File not found: FileId={FileId}", message.FileId);
                return;
            }

            file.Status = message.NewStatus;

            file.UpdatedAt = DateTime.UtcNow;

            await _writerRepository.UpdateFileAsync(file);
            
            _logger.LogInformation("File status updated successfully: FileId={FileId}, NewStatus={NewStatus}", 
                message.FileId, message.NewStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update file status: FileId={FileId}, NewStatus={NewStatus}", 
                message.FileId, message.NewStatus);
            throw;
        }
    }
} 