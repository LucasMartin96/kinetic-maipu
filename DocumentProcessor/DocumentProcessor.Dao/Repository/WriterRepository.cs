using DocumentProcessor.Contracts.Dtos;
using DocumentProcessor.Dao.Interfaces;
using DocumentProcessor.Dao.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DocumentProcessor.Dao.Repository
{
    public class WriterRepository : IWriterRepository
    {
        private readonly DocumentDbContext _context;

        public WriterRepository(DocumentDbContext context)
        {
            _context = context;
        }

        public async Task UpdateFileAsync(FileDTO file)
        {
            var entity = await _context.Files.FindAsync(file.FileId);
            if (entity == null) return;

            entity.Status = file.Status;
            entity.WordCount = file.WordCount;
            entity.LineCount = file.LineCount;
            entity.CharacterCount = file.CharacterCount;
            entity.MostFrequentWords = file.MostFrequentWords;
            entity.Summary = file.Summary;
            entity.ErrorMessage = file.ErrorMessage;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<FileDTO?> GetFileByIdAsync(Guid fileId)
        {
            var entity = await _context.Files
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FileId == fileId);

            return entity == null ? null : EntityToDtoMapper.ToDto(entity);
        }

        public async Task<IEnumerable<FileDTO>> GetFilesByProcessIdAsync(Guid processId)
        {
            var entities = await _context.Files
                .AsNoTracking()
                .Where(f => f.ProcessId == processId)
                .ToListAsync();

            return entities.Select(EntityToDtoMapper.ToDto);
        }

        public async Task CreateFilesAsync(IEnumerable<FileDTO> files)
        {
            var entities = files.Select(DtoToEntityMapper.ToEntity);
            await _context.Files.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<ProcessDTO?> GetProcessByIdAsync(Guid processId)
        {
            var entity = await _context.Processes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProcessId == processId);

            return entity == null ? null : EntityToDtoMapper.ToDto(entity);
        }

        public async Task UpdateProcessAsync(ProcessDTO process)
        {
            var entity = await _context.Processes.FindAsync(process.ProcessId);
            if (entity == null) return;

            entity.Status = process.Status;
            entity.CompletedFiles = process.CompletedFiles;
            entity.FailedFiles = process.FailedFiles;
            entity.SkippedFiles = process.SkippedFiles;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.StartedAt = process.StartedAt ?? entity.StartedAt;

            if (process.Status == "COMPLETED")
            {
                entity.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}
