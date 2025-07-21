using DocumentProcessor.Master.Interfaces;

namespace DocumentProcessor.Master.Services;

public class RetryPolicy : IResilienceRetryPolicy
{
    private readonly ILogger<RetryPolicy> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public RetryPolicy(ILogger<RetryPolicy> logger, int maxRetries = 3, TimeSpan? baseDelay = null)
    {
        _logger = logger;
        _maxRetries = maxRetries;
        _baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName)
    {
        var attempt = 0;
        var delay = _baseDelay;

        while (true)
        {
            try
            {
                attempt++;
                _logger.LogDebug("Executing {OperationName} - Attempt {Attempt}/{MaxRetries}", 
                    operationName, attempt, _maxRetries + 1);
                
                return await operation();
            }
            catch (Exception ex) when (IsTransientException(ex) && attempt <= _maxRetries)
            {
                _logger.LogWarning(ex, "Transient failure in {OperationName} - Attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms", 
                    operationName, attempt, _maxRetries + 1, delay.TotalMilliseconds);
                
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Con dos andamos
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Permanent failure in {OperationName} after {Attempt} attempts", 
                    operationName, attempt);
                throw;
            }
        }
    }

    public async Task ExecuteAsync(Func<Task> operation, string operationName)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, operationName);
    }

    private static bool IsTransientException(Exception ex)
    {
        return ex is TimeoutException ||
               ex is HttpRequestException ||
               ex is TaskCanceledException ||
               ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
    }
} 