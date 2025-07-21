using MassTransit;
using DocumentProcessor.Contracts.Commands;
using DocumentProcessor.Dao.Interfaces;

namespace DocumentProcessor.Writer.Consumers;

public class UpdateProcessStatusCommandConsumer : IConsumer<UpdateProcessStatusCommand>
{
    private readonly IWriterRepository _writerRepository;
    private readonly ILogger<UpdateProcessStatusCommandConsumer> _logger;

    public UpdateProcessStatusCommandConsumer(
        IWriterRepository writerRepository,
        ILogger<UpdateProcessStatusCommandConsumer> logger)
    {
        _writerRepository = writerRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateProcessStatusCommand> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("WRITER: Updating process status - ProcessId: {ProcessId}, NewStatus: {NewStatus}, Reason: {Reason}", 
            message.ProcessId, message.NewStatus, message.Reason);

        try
        {
            var process = await _writerRepository.GetProcessByIdAsync(message.ProcessId);
            if (process == null)
            {
                _logger.LogWarning("Process not found: {ProcessId}", message.ProcessId);
                return;
            }
            if (process.Status == "PENDING" && message.NewStatus == "RUNNING")
            {
                process.Status = "RUNNING";
                if (process.StartedAt == default)
                    process.StartedAt = DateTime.UtcNow;
                await _writerRepository.UpdateProcessAsync(process);
                _logger.LogInformation("Process {ProcessId} status set to RUNNING and StartedAt set.", process.ProcessId);
            }

            var files = await _writerRepository.GetFilesByProcessIdAsync(message.ProcessId);
            // TODO: aca el enum!!!
            var completedFiles = files.Count(f => f.Status == "COMPLETED");
            var failedFiles = files.Count(f => f.Status == "FAILED");

            process.Status = message.NewStatus;
            process.CompletedFiles = completedFiles;
            process.FailedFiles = failedFiles;
            process.UpdatedAt = DateTime.UtcNow;

            await _writerRepository.UpdateProcessAsync(process);
            
            _logger.LogInformation("WRITER: Process status updated successfully - ProcessId: {ProcessId}, NewStatus: {NewStatus}, CompletedFiles: {CompletedFiles}, FailedFiles: {FailedFiles}", 
                message.ProcessId, message.NewStatus, completedFiles, failedFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WRITER: Failed to update process status - ProcessId: {ProcessId}, NewStatus: {NewStatus}", 
                message.ProcessId, message.NewStatus);
            throw;
        }
    }
} 