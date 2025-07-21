using DocumentProcessor.Master.Interfaces;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class TimeoutHandler : ITimeoutHandler
{
    private readonly ILogger<TimeoutHandler> _logger;

    public TimeoutHandler(ILogger<TimeoutHandler> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, string operationName)
    {
        // Se crea un token de cancelación que se activa luego del timeout
        using var cts = new CancellationTokenSource(timeout);

        try
        {
            _logger.LogDebug("Executing {OperationName} with timeout of {Timeout}",
                operationName, timeout);

            // Ejecuta la operación y espera con cancelación
            var task = operation();
            var result = await task.WaitAsync(cts.Token);

            _logger.LogDebug("Successfully completed {OperationName}", operationName);
            return result;
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            // Si el token fue cancelado, se considera timeout
            _logger.LogError("Operation {OperationName} timed out after {Timeout}",
                operationName, timeout);
            throw new TimeoutException($"Operation {operationName} timed out after {timeout}");
        }
    }

    public async Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout, string operationName)
    {
        // Adapta operaciones void para reutilizar la lógica común
        await ExecuteWithTimeoutAsync(async () =>
        {
            await operation();
            return true;
        }, timeout, operationName);
    }
}
