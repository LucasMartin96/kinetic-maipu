using DocumentProcessor.Contracts.Dtos;

namespace DocumentProcessor.Dao.Interfaces
{
    // TODO: Docuemntar metodos
    public interface IWriterRepository
    {
        Task UpdateFileAsync(FileDTO file);
        Task<FileDTO?> GetFileByIdAsync(Guid fileId);
        Task<IEnumerable<FileDTO>> GetFilesByProcessIdAsync(Guid processId);
        Task CreateFilesAsync(IEnumerable<FileDTO> files);
        Task<ProcessDTO?> GetProcessByIdAsync(Guid processId);
        Task UpdateProcessAsync(ProcessDTO process);
    }
}
