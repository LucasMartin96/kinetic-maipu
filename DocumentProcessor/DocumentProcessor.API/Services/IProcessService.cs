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
        /// Detiene un proceso en ejecución, siempre que no haya finalizado aún.
        /// </summary>
        /// <param name="processId">ID del proceso</param>
        /// <returns>True si se detuvo correctamente</returns>
        Task<bool> StopProcessAsync(Guid processId);

        /// <summary>
        /// Obtiene el estado actual del proceso, porcentaje de avance y tiempo estimado de finalización.
        /// </summary>
        /// <param name="processId">ID del proceso</param>
        /// <returns>Objeto con el estado y progreso</returns>
        Task<ProcessStatusResponse> GetProcessStatusAsync(Guid processId);

        /// <summary>
        /// Lista todos los procesos existentes ordenados por fecha de creación.
        /// </summary>
        /// <returns>Lista de resúmenes de procesos</returns>
        Task<List<ProcessSummary>> ListProcessesAsync();

        /// <summary>
        /// Obtiene las métricas finales del proceso (palabras, líneas, palabras frecuentes, etc).
        /// Solo disponible si el proceso fue completado.
        /// </summary>
        /// <param name="processId">ID del proceso</param>
        /// <returns>Resultado completo del procesamiento</returns>
        Task<ProcessResultsResponse> GetProcessResultsAsync(Guid processId);
    }
}
