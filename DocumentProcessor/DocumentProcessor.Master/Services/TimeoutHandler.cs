using DocumentProcessor.Master.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Master.Services;

public class TimeoutHandler : ITimeoutHandler
{
    private readonly ILogger<TimeoutHandler> _logger;

    public TimeoutHandler(ILogger<TimeoutHandler> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, string operationName)
    {
        using var cts = new CancellationTokenSource(timeout);
        
        try
        {
            _logger.LogDebug("Executing {OperationName} with timeout of {Timeout}", 
                operationName, timeout);
            
            var task = operation();
            var result = await task.WaitAsync(cts.Token);
            
            _logger.LogDebug("Successfully completed {OperationName}", operationName);
            return result;
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            _logger.LogError("Operation {OperationName} timed out after {Timeout}", 
                operationName, timeout);
            throw new TimeoutException($"Operation {operationName} timed out after {timeout}");
        }
    }

    public async Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout, string operationName)
    {
        await ExecuteWithTimeoutAsync(async () =>
        {
            await operation();
            return true;
        }, timeout, operationName);
    }
} 