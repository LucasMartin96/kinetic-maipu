using DocumentProcessor.Dao;
using DocumentProcessor.Master;
using DocumentProcessor.Master.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/master/log.txt", rollingInterval: RollingInterval.Day)
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

            services.AddMassTransit(x =>
            {
                x.AddConsumer<ProcessStartedTestConsumer>();
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("user");
                        h.Password("password");
                    });

                    cfg.ReceiveEndpoint("process-started-queue", e =>
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
