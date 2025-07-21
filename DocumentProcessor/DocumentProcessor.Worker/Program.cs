using DocumentProcessor.Worker.Consumers;
using DocumentProcessor.Worker;
using DocumentProcessor.Worker.Services;
using MassTransit;
using Serilog;
using Serilog.Events;
using DocumentProcessor.Worker.Interfaces;

Directory.CreateDirectory("logs");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] WORKER: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/document-processor.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] WORKER: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
try
{

    Log.Information("Starting Slave Worker UP...");

    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            services.AddScoped<IFileReader, FileReader>();
            services.AddScoped<ITextProcessor, TextProcessor>();
            services.AddScoped<IStopWordsFilter, StopWordsFilter>();
            services.AddScoped<IWordFrequencyAnalyzer, WordFrequencyAnalyzer>();
            services.AddScoped<ISummaryGenerator, SummaryGenerator>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<ProcessFileCommandConsumer>();
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("user");
                        h.Password("password");
                    });

                    cfg.ReceiveEndpoint("process-files-queue", e =>
                    {
                        e.ConfigureConsumer<ProcessFileCommandConsumer>(ctx);
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
    Log.Fatal(ex, "Slave Worker host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}