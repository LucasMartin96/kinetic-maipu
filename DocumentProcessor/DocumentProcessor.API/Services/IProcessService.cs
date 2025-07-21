using DocumentProcessor.API.Models;

namespace DocumentProcessor.API.Services
{
    public interface IProcessService
    {
        /// <summary>
        /// Inicia un nuevo proceso a partir de un directorio con archivos .txt.
        /// Valida la carpeta, crea la entrada en base de datos y publica el evento de inicio.
        /// </summary>
        /// <param name="request">Contiene la ruta de la carpeta a procesar</param>
        /// <returns>Resultado con el ID del proceso</returns>
        Task<ProcessStartResult> StartProcessAsync(ProcessStartRequest request);

        /// <summary>
        /// Detiene un proceso en ejecuci�n, siempre que no haya finalizado a�n.
        /// </summary>
        /// <param name="processId">ID del proceso</param>
        /// <returns>True si se detuvo correctamente</returns>
        Task<bool> StopProcessAsync(Guid processId);

        /// <summary>
        /// Obtiene el estado actual del proceso, porcentaje de avance y tiempo estimado de finalizaci�n.
        /// </summary>
        /// <param name="processId">ID del proceso</param>
        /// <returns>Objeto con el estado y progreso</returns>
        Task<ProcessStatusResponse> GetProcessStatusAsync(Guid processId);

        /// <summary>
        /// Lista todos los procesos existentes ordenados por fecha de creaci�n.
        /// </summary>
        /// <returns>Lista de res�menes de procesos</returns>
        Task<List<ProcessSummary>> ListProcessesAsync();

        /// <summary>
        /// Obtiene las m�tricas finales del proceso (palabras, l�neas, palabras frecuentes, etc).
        /// Solo disponible si el proceso fue completado.
        /// </summary>
        /// <param name="processId">ID del proceso</param>
        /// <returns>Resultado completo del procesamiento</returns>
        Task<ProcessResultsResponse> GetProcessResultsAsync(Guid processId);
    }
}
