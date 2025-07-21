namespace DocumentProcessor.Master.Interfaces;

/// <summary>
/// Define una política para ejecutar operaciones asincrónicas con un límite de tiempo (timeout).
/// Lanza una excepción si la operación no finaliza dentro del tiempo establecido.
/// </summary>
public interface ITimeoutHandler
{
    /// <summary>
    /// Ejecuta una operación asincrónica con un límite de tiempo. Si no finaliza a tiempo, lanza <see cref="TimeoutException"/>.
    /// </summary>
    /// <typeparam name="T">Tipo de retorno de la operación.</typeparam>
    /// <param name="operation">Operación a ejecutar.</param>
    /// <param name="timeout">Tiempo máximo permitido para ejecutar la operación.</param>
    /// <param name="operationName">Nombre de la operación (para fines de logging).</param>
    /// <returns>El resultado de la operación si se completa dentro del tiempo.</returns>
    Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, string operationName);

    /// <summary>
    /// Ejecuta una operación asincrónica sin retorno con un límite de tiempo. Si no finaliza a tiempo, lanza <see cref="TimeoutException"/>.
    /// </summary>
    /// <param name="operation">Operación asincrónica a ejecutar.</param>
    /// <param name="timeout">Tiempo máximo permitido.</param>
    /// <param name="operationName">Nombre de la operación (para fines de logging).</param>
    Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout, string operationName);
}
