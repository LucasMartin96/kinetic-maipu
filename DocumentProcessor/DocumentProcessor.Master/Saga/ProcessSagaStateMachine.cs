using MassTransit;
using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Contracts.Commands;
using Microsoft.Extensions.Logging;
using DocumentProcessor.Master.Services;
using Microsoft.Extensions.DependencyInjection;
using DocumentProcessor.Master.Interfaces;

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
                .ThenAsync(async context =>
                {
                    _logger.LogInformation("SAGA: Process started - ProcessId: {ProcessId}, Files: {FileCount}", 
                        context.Message.ProcessId, context.Message.FileNames.Count);
                    
                    var serviceProvider = context.GetPayload<IServiceProvider>();
                    var serviceLocator = new ServiceLocator(serviceProvider);
                    var sagaService = serviceLocator.GetRequiredService<IProcessSagaService>();
                    
                    await sagaService.InitializeProcessAsync(context.Saga, context.Message.ProcessId, context.Message.FileNames);
                    context.Saga.State = Processing.Name;
                })
                .TransitionTo(Processing)
        );

        During(Processing,
            When(FileReady)
                .ThenAsync(async context =>
                {
                    _logger.LogInformation("SAGA: File ready for processing - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}", 
                        context.Message.ProcessId, context.Message.FileId, context.Message.FileName);
                    
                    var serviceProvider = context.GetPayload<IServiceProvider>();
                    var serviceLocator = new ServiceLocator(serviceProvider);
                    var sagaService = serviceLocator.GetRequiredService<IProcessSagaService>();
                    
                    await sagaService.HandleFileReadyAsync(context.Saga, context.Message.FileId, context.Message.FileName);
                })
        );

        During(Processing,
            When(FileProcessed)
                .ThenAsync(async context =>
                {
                    _logger.LogInformation("SAGA: File processed - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}", 
                        context.Message.ProcessId, context.Message.FileId, context.Message.FileName);
                    
                    var serviceProvider = context.GetPayload<IServiceProvider>();
                    var serviceLocator = new ServiceLocator(serviceProvider);
                    var sagaService = serviceLocator.GetRequiredService<IProcessSagaService>();
                    
                    await sagaService.HandleFileProcessedAsync(context.Saga, context.Message);
                })
        );

        During(Processing,
            When(FilePersisted)
                .ThenAsync(async context =>
                {
                    _logger.LogInformation("SAGA: File persisted - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}, Status: {Status}", 
                        context.Message.ProcessId, context.Message.FileId, context.Message.FileName, context.Message.Status);
                    
                    var serviceProvider = context.GetPayload<IServiceProvider>();
                    var serviceLocator = new ServiceLocator(serviceProvider);
                    var sagaService = serviceLocator.GetRequiredService<IProcessSagaService>();
                    
                    await sagaService.HandleFilePersistedAsync(context.Saga, context.Message.FileId, context.Message.FileName, context.Message.Status);
                })
                .If(context => context.Saga.PersistedFiles + context.Saga.FailedFiles >= context.Saga.TotalFiles,
                    b => b.Then(context =>
                    {
                        context.Saga.State = Completed.Name;
                        _logger.LogInformation("SAGA: All files processed - ProcessId: {ProcessId}, Persisted: {Persisted}, Failed: {Failed}, Total: {Total}", 
                            context.Message.ProcessId, context.Saga.PersistedFiles, context.Saga.FailedFiles, context.Saga.TotalFiles);
                    })
                    .PublishAsync(context => context.Init<UpdateProcessStatusCommand>(new
                    {
                        ProcessId = context.Saga.ProcessId,
                        NewStatus = context.Saga.FailedFiles > 0 ? "COMPLETED_WITH_FAILURES" : "COMPLETED",
                        Reason = context.Saga.FailedFiles > 0 
                            ? $"Process completed with {context.Saga.FailedFiles} failures" 
                            : "All files processed successfully"
                    }))
                    .Then(context => _logger.LogInformation("SAGA: Published UpdateProcessStatusCommand for {ProcessId}", context.Saga.ProcessId))
                    .TransitionTo(Completed).Finalize())
        );

        During(Processing,
            When(FileFailed)
                .ThenAsync(async context =>
                {
                    _logger.LogError("SAGA: File processing failed - ProcessId: {ProcessId}, FileId: {FileId}, FileName: {FileName}, Error: {Error}", 
                        context.Message.ProcessId, context.Message.FileId, context.Message.FileName, context.Message.ErrorMessage);
                    
                    var serviceProvider = context.GetPayload<IServiceProvider>();
                    var serviceLocator = new ServiceLocator(serviceProvider);
                    var sagaService = serviceLocator.GetRequiredService<IProcessSagaService>();
                    await sagaService.HandleFileFailedAsync(context.Saga, context.Message);
                })
        );

        SetCompletedWhenFinalized();
    }
} 