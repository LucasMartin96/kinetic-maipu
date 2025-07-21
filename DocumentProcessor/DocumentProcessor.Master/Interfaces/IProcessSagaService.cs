using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentProcessor.Master.Interfaces
{
    public interface IProcessSagaService
    {
        /// <summary>
        /// Inicializa el proceso Saga estableciendo el estado inicial y publicando los comandos necesarios.
        /// </summary>
        /// <param name="saga">Estado actual del Saga</param>
        /// <param name="processId">Identificador único del proceso</param>
        /// <param name="fileNames">Lista de nombres de archivos a procesar</param>
        Task InitializeProcessAsync(ProcessSagaState saga, Guid processId, List<string> fileNames);

        /// <summary>
        /// Maneja la señal que indica que un archivo está listo para ser procesado,
        /// publicando el comando para iniciar su procesamiento.
        /// </summary>
        Task HandleFileReadyAsync(ProcessSagaState saga, Guid fileId, string fileName);

        /// <summary>
        /// Maneja la notificación de que un archivo fue procesado,
        /// publicando el comando para persistir los resultados.
        /// </summary>
        Task HandleFileProcessedAsync(ProcessSagaState saga, FileProcessedEvent fileProcessedEvent);

        /// <summary>
        /// Actualiza el estado cuando un archivo fue persistido en la base de datos,
        /// incrementando el contador de archivos persistidos.
        /// </summary>
        Task HandleFilePersistedAsync(ProcessSagaState saga, Guid fileId, string fileName, string status);

        /// <summary>
        /// Maneja la notificación de fallo en el procesamiento de un archivo,
        /// publicando el comando para persistir el error y actualizar estado.
        /// </summary>
        Task HandleFileFailedAsync(ProcessSagaState saga, FileFailedEvent fileFailedEvent);

        /// <summary>
        /// Verifica si el proceso Saga ha completado el procesamiento de todos los archivos.
        /// </summary>
        /// <returns>True si el proceso está completo, false si no</returns>
        Task<bool> IsProcessCompletedAsync(ProcessSagaState saga);

        /// <summary>
        /// Obtiene el estado actual (string) del proceso Saga.
        /// </summary>
        Task<string> GetProcessStatusAsync(ProcessSagaState saga);
    }
}
