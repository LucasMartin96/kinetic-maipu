using MassTransit;
using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Contracts.Events;
using DocumentProcessor.Contracts.Commands;
using DocumentProcessor.Master.Services;
using DocumentProcessor.Master.Interfaces;
using DocumentProcessor.Contracts;

namespace DocumentProcessor.Master.Saga;

/// <summary>
/// Saga State Machine para orquestar el procesamiento de documentos.
/// Coordina los eventos del proceso (inicio, procesamiento de archivos, persistencia, fallos)
/// y realiza transiciones de estado según el avance del mismo.
/// </summary>
public class ProcessSagaStateMachine : MassTransitStateMachine<ProcessSagaState>
{
    /// <summary>
    /// Estado intermedio: El proceso está en ejecución y esperando que todos los archivos se completen.
    /// </summary>
    public State Processing { get; private set; } = null!;

    /// <summary>
    /// Estado final: El proceso ha finalizado, exitosamente o con errores.
    /// </summary>
    public State Completed { get; private set; } = null!;

    // Eventos recibidos por la saga
    public Event<ProcessStartedEvent> ProcessStarted { get; private set; } = null!;
    public Event<FileReadyEvent> FileReady { get; private set; } = null!;
    public Event<FileProcessedEvent> FileProcessed { get; private set; } = null!;
    public Event<FilePersistedEvent> FilePersisted { get; private set; } = null!;
    public Event<FileFailedEvent> FileFailed { get; private set; } = null!;

    private readonly ILogger<ProcessSagaStateMachine> _logger;

    /// <summary>
    /// Constructor de la máquina de estados.
    /// Define los eventos, correlaciones y transiciones de estado del proceso.
    /// </summary>
    public ProcessSagaStateMachine(ILogger<ProcessSagaStateMachine> logger)
    {
        _logger = logger;

        // Configura el estado de la instancia (en la propiedad State del Saga)
        InstanceState(x => x.State);

        // Correlación de eventos usando el ProcessId
        Event(() => ProcessStarted, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FileReady, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FileProcessed, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FilePersisted, e => e.CorrelateById(context => context.Message.ProcessId));
        Event(() => FileFailed, e => e.CorrelateById(context => context.Message.ProcessId));

        // TRANSICIÓN INICIAL: Proceso iniciado
        Initially(
            When(ProcessStarted)
                .ThenAsync(async context =>
                {
                    _logger.LogInformation("SAGA: Process started - ProcessId: {ProcessId}, Files: {FileCount}",
                        context.Message.ProcessId, context.Message.FileNames.Count);

                    // Resolve service para lógica de negocio
                    var serviceProvider = context.GetPayload<IServiceProvider>();
                    var serviceLocator = new ServiceLocator(serviceProvider);
                    var sagaService = serviceLocator.GetRequiredService<IProcessSagaService>();

                    // Inicializa el proceso (crea archivos, inicializa contadores, etc.)
                    await sagaService.InitializeProcessAsync(context.Saga, context.Message.ProcessId, context.Message.FileNames);

                    // Establece estado a Processing
                    context.Saga.State = Processing.Name;
                })
                .TransitionTo(Processing) // Transición al estado "Processing"
        );

        // EVENTO: Un archivo está listo para ser procesado
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

        // EVENTO: Un archivo ha sido procesado (pero aún no persistido)
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

        // EVENTO: Un archivo fue persistido correctamente
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
                    b => b
                        // Si ya se procesaron todos los archivos (incluyendo fallidos), se finaliza el proceso
                        .Then(context =>
                        {
                            context.Saga.State = Completed.Name;

                            _logger.LogInformation("SAGA: All files processed - ProcessId: {ProcessId}, Persisted: {Persisted}, Failed: {Failed}, Total: {Total}",
                                context.Message.ProcessId, context.Saga.PersistedFiles, context.Saga.FailedFiles, context.Saga.TotalFiles);
                        })
                        // Publica el comando para actualizar el estado final del proceso
                        .PublishAsync(context => context.Init<UpdateProcessStatusCommand>(new
                        {
                            ProcessId = context.Saga.ProcessId,
                            NewStatus = context.Saga.FailedFiles > 0
                                ? ProcessStatus.CompletedWithFailures
                                : ProcessStatus.Completed,
                            Reason = context.Saga.FailedFiles > 0
                                ? $"Process completed with {context.Saga.FailedFiles} failures"
                                : "All files processed successfully"
                        }))
                        .Then(context => _logger.LogInformation("SAGA: Published UpdateProcessStatusCommand for {ProcessId}", context.Saga.ProcessId))
                        .TransitionTo(Completed) // Transición a estado final
                        .Finalize()
                )
        );

        // EVENTO: Fallo al procesar un archivo
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

        // Marca el Saga como completado cuando se alcanza el estado Final
        SetCompletedWhenFinalized();
    }
}
