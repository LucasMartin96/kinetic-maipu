using DocumentProcessor.Contracts.TestsRecords;
using MassTransit;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Master.Consumers;
public class ProcessStartedTestConsumer : IConsumer<ProcessStartedTestEvent>
{
    private readonly ILogger<ProcessStartedTestConsumer> _logger;

    public ProcessStartedTestConsumer(ILogger<ProcessStartedTestConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessStartedTestEvent> context)
    {
        _logger.LogInformation("Received event ProcessStartedEvent: {ProcessId}", context.Message.ProcessId);

        var command = new ProcessFilesTestSlaveCommand
        {
            ProcessId = context.Message.ProcessId,
            Files = new[] { "file1.txt", "file2.txt" }
        };

        var destinationAddress = new Uri("queue:process-files-queue");

        var endpoint = await context.GetSendEndpoint(destinationAddress);
        await endpoint.Send(command);

        _logger.LogInformation("Sent command ProcessFilesTestSlaveCommand for ProcessId: {ProcessId}", command.ProcessId);
    }
}