namespace DocumentProcessor.Master.Interfaces;

public interface ITimeoutHandler
{
    Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> operation, TimeSpan timeout, string operationName);
    Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout, string operationName);
} 