using DocumentProcessor.Master.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.Master.Services;

public class ServiceLocator : IServiceLocator
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceLocator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T? GetService<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }

    public T GetRequiredService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }
} 