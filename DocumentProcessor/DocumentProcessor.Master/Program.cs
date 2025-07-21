using DocumentProcessor.Dao;
using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Master;
using DocumentProcessor.Master.Consumers;
using DocumentProcessor.Master.Interfaces;
using DocumentProcessor.Master.Saga;
using DocumentProcessor.Master.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Directory.CreateDirectory("logs");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] MASTER: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/document-processor.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] MASTER: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();



try
{
    Log.Information("Starting Master service...");
    
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DocumentDbContext>(options =>
            options.UseMySql(
               connectionString,
               ServerVersion.AutoDetect(connectionString))
            );

            services.AddScoped<IServiceLocator, ServiceLocator>();
            services.AddScoped<IProcessSagaService, ProcessSagaService>();
            services.AddScoped<IResilienceRetryPolicy, RetryPolicy>();
            services.AddScoped<ITimeoutHandler, TimeoutHandler>();

            services.AddMassTransit(x =>
            {
                // TODO: Usamos en memoria porque hay problemitas de dependencias con Pomelo... mover a mysql
                x.AddSagaStateMachine<ProcessSagaStateMachine, ProcessSagaState>()
                    .InMemoryRepository();

                x.AddConsumer<ProcessStartedTestConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("user");
                        h.Password("password");
                    });

                    cfg.ReceiveEndpoint("process-saga", e =>
                    {
                        e.ConfigureSaga<ProcessSagaState>(ctx);
                    });

                    cfg.ReceiveEndpoint("test-events", e =>
                    {
                        e.ConfigureConsumer<ProcessStartedTestConsumer>(ctx);
                    });
                });
            });

            services.AddHostedService<Worker>();
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Master service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
