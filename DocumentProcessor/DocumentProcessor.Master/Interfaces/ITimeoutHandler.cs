namespace DocumentProcessor.Master.Interfaces;

/// <summary>
/// Define una pol�tica para ejecutar operaciones asincr�nicas con un l�mite de tiempo (timeout).
/// Lanza una excepci�n si la operaci�n no finaliza dentro del tiempo establecido.
/// </summary>
public interface ITimeoutHandler
{
    /// <summary>
    /// Ejecuta una operaci�n asincr�nica con un l�mite de tiempo. Si no finaliza a tiempo, lanza <see cref="TimeoutException"/>.
    /// </summary>
    /// <typeparam name="T">Tipo de retorno de la operaci�n.</typeparam>
    /// <param name="operation">Operaci�n a ejecutar.</param>
    /// <param name="timeout">Tiempo m�ximo permitido para ejecutar la operaci�n.</param>
    /// <param name="operationName">Nombre de la operaci�n (para fines de logging).</param>
    /// <returns>El resultado de la operaci�n si se completa dentro del tiempo.</returns>
    Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, string operationName);

    /// <summary>
    /// Ejecuta una operaci�n asincr�nica sin retorno con un l�mite de tiempo. Si no finaliza a tiempo, lanza <see cref="TimeoutException"/>.
    /// </summary>
    /// <param name="operation">Operaci�n asincr�nica a ejecutar.</param>
    /// <param name="timeout">Tiempo m�ximo permitido.</param>
    /// <param name="operationName">Nombre de la operaci�n (para fines de logging).</param>
    Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout, string operationName);
}
