using MassTransit;
using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Contracts.Commands;
using Microsoft.Extensions.Logging;

namespace DocumentProcessor.Master.Saga;

public class ProcessSagaStateMachine : 
    MassTransitStateMachine<ProcessSagaState>
{
    public State Processing { get; private set; } = null!;
    public State Completed { get; private set; } = null!;

    public Event<ProcessStartedEvent> ProcessStarted { get; private set; } = null!;
    public Event<FileReadyEvent> FileReady { get; private set; } = null!;
    public Event<FileProcessedEvent> FileProcessed { get; private set; } = null!;
    public Event<FilePersistedEvent> FilePersisted { get; private set; } = null!;
    public Event<FileFailedEvent> FileFailed { get; private set; } = null!;

    private readonly ILogger<ProcessSagaStateMachine> _logger;

    public ProcessSagaStateMachine(ILogger<ProcessSagaStateMachine> logger)
    {
        _logger = logger;
        
        InstanceState(x => x.State);

        Event(() => ProcessStarted, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FileReady, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FileProcessed, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FilePersisted, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FileFailed, e => e.CorrelateById(context => context.Message.ProcessId));

        Initially(
            When(ProcessStarted)
                .Then(context =>
                {
                    _logger.LogInformation("SAGA: Process started - ProcessId: {ProcessId}, Files: {FileCount}", 
                        context.Message.ProcessId, context.Message.FileNames.Count);
                    
                    context.Saga.CorrelationId = context.Message.ProcessId;
                    context.Saga.ProcessId = context.Message.ProcessId;
                    context.Saga.TotalFiles = context.Message.FileNames.Count;
                    context.Saga.CurrentState = "Processing";
                    context.Saga.State = Processing.Name;
                    context.Saga.CreatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("SAGA: Initialized saga state - TotalFiles: {TotalFiles}, State: {State}", 
                        context.Saga.TotalFiles, context.Saga.State);
                })
                .TransitionTo(Processing)
                .PublishAsync(context => context.Init<InitializeFilesCommand>(new
                {
                    ProcessId = context.Message.ProcessId,
                    FileNames = context.Message.FileNames
                }))
                .Then(context => _logger.LogInformation("SAGA: Published InitializeFilesCommand for {ProcessId}", context.Message.ProcessId))
        );

        During(Processing,
            When(FileReady)
                .Then(context =>
                {
                    _logger.LogInformation("SAGA: File ready for processing - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}", 
                        context.Message.ProcessId, context.Message.FileId, context.Message.FileName);
                })
                .PublishAsync(context => context.Init<ProcessFileCommand>(new
                {
                    ProcessId = context.Saga.ProcessId,
                    FileId = context.Message.FileId,
                    FileName = context.Message.FileName
                }))
                .Then(context => _logger.LogInformation("SAGA: Published ProcessFileCommand for {FileName}", context.Message.FileName))
        );

        During(Processing,
            When(FileProcessed)
                .Then(context =>
                {
                    _logger.LogInformation("SAGA: File processed - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}", 
                        context.Message.ProcessId, context.Message.FileId, context.Message.FileName);
                    
                    context.Saga.CompletedFiles++;
                    context.Saga.UpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("SAGA: Progress update - CompletedFiles: {CompletedFiles}/{TotalFiles}", 
                        context.Saga.CompletedFiles, context.Saga.TotalFiles);
                })
                .PublishAsync(context => context.Init<PersistFileResultCommand>(new
                {
                    ProcessId = context.Saga.ProcessId,
                    FileId = context.Message.FileId,
                    FileName = context.Message.FileName,
                    Status = "COMPLETED",
                    WordCount = context.Message.WordCount,
                    LineCount = context.Message.LineCount,
                    CharacterCount = context.Message.CharacterCount,
                    MostFrequentWords = context.Message.MostFrequentWords,
                    Summary = context.Message.Summary
                }))
                .Then(context => _logger.LogInformation("SAGA: Published PersistFileResultCommand for {FileName}", context.Message.FileName))
        );

        During(Processing,
            When(FilePersisted)
                .Then(context =>
                {
                    context.Saga.PersistedFiles++;
                    _logger.LogInformation("SAGA: File persisted - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}, Status: {Status}, PersistedFiles: {PersistedFiles}/{TotalFiles}", 
                        context.Message.ProcessId, context.Message.FileId, context.Message.FileName, context.Message.Status, context.Saga.PersistedFiles, context.Saga.TotalFiles);
                })
                .If(context => context.Saga.PersistedFiles >= context.Saga.TotalFiles,
                    b => b.Then(context =>
                    {
                        context.Saga.State = Completed.Name;
                        _logger.LogInformation("SAGA: All files processed and persisted - ProcessId: {ProcessId}", context.Message.ProcessId);
                    })
                    .PublishAsync(context => context.Init<UpdateProcessStatusCommand>(new
                    {
                        ProcessId = context.Saga.ProcessId,
                        NewStatus = "COMPLETED",
                        Reason = "All files processed successfully"
                    }))
                    .Then(context => _logger.LogInformation("SAGA: Published UpdateProcessStatusCommand for {ProcessId}", context.Saga.ProcessId))
                    .TransitionTo(Completed).Finalize())
        );

        SetCompletedWhenFinalized();
    }
} 