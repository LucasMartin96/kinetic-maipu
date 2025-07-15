using DocumentProcessor.Worker.Consumers;
using DocumentProcessor.Worker;
using MassTransit;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/master/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{

    Log.Information("Starting Slave Worker UP...");

    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ProcessFilesTestSlaveConsumer>();
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("user");
                        h.Password("password");
                    });

                    cfg.ReceiveEndpoint("process-files-queue", e =>
                    {
                        e.ConfigureConsumer<ProcessFilesTestSlaveConsumer>(ctx);
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