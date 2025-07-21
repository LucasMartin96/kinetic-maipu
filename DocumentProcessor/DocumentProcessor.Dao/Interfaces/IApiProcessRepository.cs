using DocumentProcessor.Contracts.Dtos;

namespace DocumentProcessor.Dao.Interfaces
{
    // TODO: Docuemntar metodos.
    public interface IApiProcessRepository
    {
        Task<Guid> CreateProcessAsync(ProcessDTO process);
        Task<ProcessDTO?> GetProcessByIdAsync(Guid processId);
        Task<bool> UpdateProcessAsync(ProcessDTO process);
        Task<List<ProcessDTO>> GetAllProcessesAsync();
    }
}
