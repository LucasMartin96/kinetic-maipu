using DocumentProcessor.Contracts.TestsRecords;
using MassTransit;

namespace DocumentProcessor.Worker.Consumers;

public class ProcessFilesTestSlaveConsumer : IConsumer<ProcessFilesTestSlaveCommand>
{
    private readonly ILogger<ProcessFilesTestSlaveConsumer> _logger;

    public ProcessFilesTestSlaveConsumer(ILogger<ProcessFilesTestSlaveConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessFilesTestSlaveCommand> context)
    {
        var processId = context.Message.ProcessId;
        var files = context.Message.Files;

        _logger.LogInformation("Worker received command to process {FileCount} files for process {ProcessId}", files.Length, processId);

        foreach (var file in files)
        {
            _logger.LogInformation("Processing file {File}", file);

            await context.Publish(new FileProcessedTestEvent
            {
                ProcessId = processId,
                FileName = file
            });
        }

        await context.Publish(new ProcessCompletedTestEvent
        {
            ProcessId = processId
        });
    }
}
