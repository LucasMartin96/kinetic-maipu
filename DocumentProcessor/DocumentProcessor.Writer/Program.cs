using DocumentProcessor.Dao;
using DocumentProcessor.Writer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/Writer/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{

    Log.Information("Starting Writer Worker UP...");

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
                // TODO: Agregar consumers!!! 
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("user");
                        h.Password("password");
                    });

                    cfg.ReceiveEndpoint("process-files-queue", e =>
                    {

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