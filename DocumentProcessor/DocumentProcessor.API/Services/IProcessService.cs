using DocumentProcessor.API.Models;

namespace DocumentProcessor.API.Services
{
    public interface IProcessService
    {
        Task<ProcessStartResult> StartProcessAsync(ProcessStartRequest request);
        Task<bool> StopProcessAsync(Guid processId);
        Task<ProcessStatusResponse> GetProcessStatusAsync(Guid processId);
        Task<List<ProcessSummary>> ListProcessesAsync();
        Task<ProcessResultsResponse> GetProcessResultsAsync(Guid processId);
    }
} 