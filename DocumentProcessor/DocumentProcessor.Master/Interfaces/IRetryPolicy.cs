namespace DocumentProcessor.Master.Interfaces;

public interface IResilienceRetryPolicy
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName);
    Task ExecuteAsync(Func<Task> operation, string operationName);
} 