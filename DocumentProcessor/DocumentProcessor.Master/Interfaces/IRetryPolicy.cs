namespace DocumentProcessor.Master.Interfaces;

/// <summary>
/// Define una pol�tica de reintentos resiliente para operaciones asincr�nicas.
/// Permite ejecutar funciones con reintentos autom�ticos ante fallos transitorios.
/// </summary>
public interface IResilienceRetryPolicy
{
    /// <summary>
    /// Ejecuta una operaci�n asincr�nica que retorna un valor, con reintentos en caso de error transitorio.
    /// </summary>
    /// <typeparam name="T">Tipo de resultado esperado.</typeparam>
    /// <param name="operation">Funci�n asincr�nica a ejecutar.</param>
    /// <param name="operationName">Nombre de la operaci�n para trazabilidad en logs.</param>
    /// <returns>El resultado de la operaci�n.</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName);

    /// <summary>
    /// Ejecuta una operaci�n asincr�nica sin retorno, con reintentos en caso de error transitorio.
    /// </summary>
    /// <param name="operation">Acci�n asincr�nica a ejecutar.</param>
    /// <param name="operationName">Nombre de la operaci�n para trazabilidad en logs.</param>
    Task ExecuteAsync(Func<Task> operation, string operationName);
}
