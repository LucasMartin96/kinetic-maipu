using DocumentProcessor.Contracts.Dtos;
using DocumentProcessor.Contracts;
using DocumentProcessor.Dao.Entities;
using DocumentProcessor.Dao.Interfaces;
using DocumentProcessor.Dao.Mappers;
using Microsoft.EntityFrameworkCore;

namespace DocumentProcessor.Dao.Repository
{
    public class ApiProcessRepository : IApiProcessRepository
    {
        private readonly DocumentDbContext _context;

        public ApiProcessRepository(DocumentDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateProcessAsync(ProcessDTO processDto)
        {
            var entity = new Process
            {
                ProcessId = Guid.NewGuid(),
                Status = processDto.Status ?? ProcessStatus.Pending,
                TotalFiles = processDto.TotalFiles,
                CompletedFiles = 0,
                FailedFiles = 0,
                SkippedFiles = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                FolderPath = processDto.FolderPath
            };

            _context.Processes.Add(entity);
            await _context.SaveChangesAsync();

            return entity.ProcessId;
        }

        public async Task<ProcessDTO?> GetProcessByIdAsync(Guid processId)
        {
            var entity = await _context.Processes
                .Include(p => p.Files)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProcessId == processId);

            if (entity == null)
                return null;

            return EntityToDtoMapper.ToDto(entity);
        }

        public async Task<bool> UpdateProcessAsync(ProcessDTO processDto)
        {
            var entity = await _context.Processes.FindAsync(processDto.ProcessId);
            if (entity == null)
                return false;

            entity.Status = processDto.Status ?? entity.Status;
            entity.TotalFiles = processDto.TotalFiles;
            entity.CompletedFiles = processDto.CompletedFiles;
            entity.FailedFiles = processDto.FailedFiles;
            entity.SkippedFiles = processDto.SkippedFiles;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProcessDTO>> GetAllProcessesAsync()
        {
            var entities = await _context.Processes
                .Include(p => p.Files)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return entities.Select(EntityToDtoMapper.ToDto).ToList();
        }
    }
}
