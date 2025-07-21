namespace DocumentProcessor.Master.Interfaces;

/// <summary>
/// Define una política de reintentos resiliente para operaciones asincrónicas.
/// Permite ejecutar funciones con reintentos automáticos ante fallos transitorios.
/// </summary>
public interface IResilienceRetryPolicy
{
    /// <summary>
    /// Ejecuta una operación asincrónica que retorna un valor, con reintentos en caso de error transitorio.
    /// </summary>
    /// <typeparam name="T">Tipo de resultado esperado.</typeparam>
    /// <param name="operation">Función asincrónica a ejecutar.</param>
    /// <param name="operationName">Nombre de la operación para trazabilidad en logs.</param>
    /// <returns>El resultado de la operación.</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName);

    /// <summary>
    /// Ejecuta una operación asincrónica sin retorno, con reintentos en caso de error transitorio.
    /// </summary>
    /// <param name="operation">Acción asincrónica a ejecutar.</param>
    /// <param name="operationName">Nombre de la operación para trazabilidad en logs.</param>
    Task ExecuteAsync(Func<Task> operation, string operationName);
}
