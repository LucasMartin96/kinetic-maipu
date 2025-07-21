using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.Master.Interfaces;

public interface IServiceLocator
{
    T? GetService<T>() where T : class;
    T GetRequiredService<T>() where T : class;
} 