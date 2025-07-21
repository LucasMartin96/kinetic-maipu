using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Contracts.Commands;
using DocumentProcessor.Contracts.Events;
using MassTransit;
using DocumentProcessor.Master.Interfaces;
using DocumentProcessor.Contracts;

namespace DocumentProcessor.Master.Services
{
    public class ProcessSagaService : IProcessSagaService
    {
        private readonly ILogger<ProcessSagaService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IResilienceRetryPolicy _retryPolicy;
        private readonly ITimeoutHandler _timeoutHandler;

        public ProcessSagaService(
            ILogger<ProcessSagaService> logger,
            IPublishEndpoint publishEndpoint,
            IResilienceRetryPolicy retryPolicy,
            ITimeoutHandler timeoutHandler)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _retryPolicy = retryPolicy;
            _timeoutHandler = timeoutHandler;
        }

     
        public async Task InitializeProcessAsync(ProcessSagaState saga, Guid processId, List<string> fileNames)
        {
            await _timeoutHandler.ExecuteWithTimeoutAsync(async () =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("SAGA SERVICE: Initializing process - ProcessId: {ProcessId}, Files: {FileCount}",
                        processId, fileNames.Count);

                    saga.CorrelationId = processId;
                    saga.ProcessId = processId;
                    saga.TotalFiles = fileNames.Count;
                    saga.CurrentState = "Processing";
                    saga.CreatedAt = DateTime.UtcNow;

                    _logger.LogInformation("SAGA SERVICE: Process initialized - TotalFiles: {TotalFiles}, State: {State}",
                        saga.TotalFiles, saga.CurrentState);

                    // Publica comando para que los workers creen los registros de archivos
                    await _publishEndpoint.Publish(new InitializeFilesCommand
                    {
                        ProcessId = processId,
                        FileNames = fileNames
                    });

                    _logger.LogInformation("SAGA SERVICE: Published InitializeFilesCommand for {ProcessId}", processId);
                }, OperationNames.InitializeProcess);
            }, TimeSpan.FromSeconds(5), OperationNames.InitializeProcess);
        }

        public async Task HandleFileReadyAsync(ProcessSagaState saga, Guid fileId, string fileName)
        {
            await _timeoutHandler.ExecuteWithTimeoutAsync(async () =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("SAGA SERVICE: File ready for processing - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}",
                        saga.ProcessId, fileId, fileName);

                    await _publishEndpoint.Publish(new ProcessFileCommand
                    {
                        ProcessId = saga.ProcessId,
                        FileId = fileId,
                        FileName = fileName
                    });

                    _logger.LogInformation("SAGA SERVICE: Published ProcessFileCommand for {FileName}", fileName);
                }, OperationNames.HandleFileReady);
            }, TimeSpan.FromSeconds(5), OperationNames.HandleFileReady);
        }

        public async Task HandleFileProcessedAsync(ProcessSagaState saga, FileProcessedEvent fileProcessedEvent)
        {
            await _timeoutHandler.ExecuteWithTimeoutAsync(async () =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogInformation("SAGA SERVICE: File processed - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}",
                        saga.ProcessId, fileProcessedEvent.FileId, fileProcessedEvent.FileName);

                    await _publishEndpoint.Publish(new PersistFileResultCommand
                    {
                        ProcessId = saga.ProcessId,
                        FileId = fileProcessedEvent.FileId,
                        FileName = fileProcessedEvent.FileName,
                        Status = ProcessStatus.Completed,
                        WordCount = fileProcessedEvent.WordCount,
                        LineCount = fileProcessedEvent.LineCount,
                        CharacterCount = fileProcessedEvent.CharacterCount,
                        MostFrequentWords = fileProcessedEvent.MostFrequentWords,
                        Summary = fileProcessedEvent.Summary
                    });

                    saga.CompletedFiles++;
                    saga.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation("SAGA SERVICE: Progress update - CompletedFiles: {CompletedFiles}/{TotalFiles}",
                        saga.CompletedFiles, saga.TotalFiles);
                    _logger.LogInformation("SAGA SERVICE: Published PersistFileResultCommand for {FileName}", fileProcessedEvent.FileName);
                }, OperationNames.HandleFileProcessed);
            }, TimeSpan.FromSeconds(5), OperationNames.HandleFileProcessed);
        }

        public async Task HandleFilePersistedAsync(ProcessSagaState saga, Guid fileId, string fileName, string status)
        {
            saga.PersistedFiles++;
            _logger.LogInformation("SAGA SERVICE: File persisted - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}, Status: {Status}, PersistedFiles: {PersistedFiles}/{TotalFiles}",
                saga.ProcessId, fileId, fileName, status, saga.PersistedFiles, saga.TotalFiles);
        }

        public async Task HandleFileFailedAsync(ProcessSagaState saga, FileFailedEvent fileFailedEvent)
        {
            await _timeoutHandler.ExecuteWithTimeoutAsync(async () =>
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    _logger.LogError("SAGA SERVICE: File processing failed - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}, Error: {Error}",
                        saga.ProcessId, fileFailedEvent.FileId, fileFailedEvent.FileName, fileFailedEvent.ErrorMessage);

                    await _publishEndpoint.Publish(new PersistFileResultCommand
                    {
                        ProcessId = saga.ProcessId,
                        FileId = fileFailedEvent.FileId,
                        FileName = fileFailedEvent.FileName,
                        Status = ProcessStatus.Failed,
                        ErrorMessage = fileFailedEvent.ErrorMessage
                    });

                    saga.FailedFiles++;
                    saga.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation("SAGA SERVICE: Failure tracked - FailedFiles: {FailedFiles}, TotalFiles: {TotalFiles}",
                        saga.FailedFiles, saga.TotalFiles);
                    _logger.LogInformation("SAGA SERVICE: Published PersistFileResultCommand for failed {FileName}", fileFailedEvent.FileName);
                }, OperationNames.HandleFileFailed);
            }, TimeSpan.FromSeconds(5), OperationNames.HandleFileFailed);
        }

        public async Task<bool> IsProcessCompletedAsync(ProcessSagaState saga)
        {
            return saga.PersistedFiles >= saga.TotalFiles;
        }

        public async Task<string> GetProcessStatusAsync(ProcessSagaState saga)
        {
            return saga.CurrentState ?? "Unknown";
        }
    }
}