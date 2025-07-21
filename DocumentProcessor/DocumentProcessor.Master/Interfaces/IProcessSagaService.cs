using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Contracts.Events;

namespace DocumentProcessor.Master.Interfaces;

public interface IProcessSagaService
{
    Task InitializeProcessAsync(ProcessSagaState saga, Guid processId, List<string> fileNames);
    Task HandleFileReadyAsync(ProcessSagaState saga, Guid fileId, string fileName);
    Task HandleFileProcessedAsync(ProcessSagaState saga, FileProcessedEvent fileProcessedEvent);
    Task HandleFilePersistedAsync(ProcessSagaState saga, Guid fileId, string fileName, string status);
    Task HandleFileFailedAsync(ProcessSagaState saga, FileFailedEvent fileFailedEvent);
    Task<bool> IsProcessCompletedAsync(ProcessSagaState saga);
    Task<string> GetProcessStatusAsync(ProcessSagaState saga);
} 