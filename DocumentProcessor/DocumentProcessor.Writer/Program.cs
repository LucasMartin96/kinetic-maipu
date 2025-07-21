using DocumentProcessor.Dao;
using DocumentProcessor.Dao.Interfaces;
using DocumentProcessor.Dao.Repository;
using DocumentProcessor.Writer;
using DocumentProcessor.Writer.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Directory.CreateDirectory("logs");


// TODO: template demasiado mal puesta aca... me zafa por ahora. Mejorar.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] WRITER: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/document-processor.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] WRITER: {Message:lj}{NewLine}{Exception}")
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

            services.AddScoped<IWriterRepository, WriterRepository>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<InitializeFilesCommandConsumer>();
                x.AddConsumer<PersistFileResultCommandConsumer>();
                x.AddConsumer<UpdateProcessStatusCommandConsumer>();
                
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("user");
                        h.Password("password");
                    });

                    cfg.ReceiveEndpoint("writer-queue", e =>
                    {
                        e.ConfigureConsumer<InitializeFilesCommandConsumer>(ctx);
                        e.ConfigureConsumer<PersistFileResultCommandConsumer>(ctx);
                        e.ConfigureConsumer<UpdateProcessStatusCommandConsumer>(ctx);
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
    Log.Fatal(ex, "Writer worker host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}