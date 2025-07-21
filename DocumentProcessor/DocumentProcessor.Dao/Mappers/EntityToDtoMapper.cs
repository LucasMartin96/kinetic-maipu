using DocumentProcessor.Contracts.Dtos;
using DocumentProcessor.Dao.Entities;


namespace DocumentProcessor.Dao.Mappers
{
    public static class EntityToDtoMapper
    {
        public static ProcessDTO ToDto(Process entity)
        {
            if (entity == null) return null!;

            var dto = new ProcessDTO
            {
                ProcessId = entity.ProcessId,
                Status = entity.Status,
                TotalFiles = entity.TotalFiles,
                CompletedFiles = entity.CompletedFiles,
                FailedFiles = entity.FailedFiles,
                SkippedFiles = entity.SkippedFiles,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                StartedAt = entity.StartedAt,
                FolderPath = entity.FolderPath,
                Files = entity.Files?.Select(f => ToDto(f)).ToList() ?? new List<FileDTO>()
            };
            return dto;
        }

        public static FileDTO ToDto(Entities.File entity)
        {
            if (entity == null) return null!;

            return new FileDTO
            {
                FileId = entity.FileId,
                ProcessId = entity.ProcessId,
                FileName = entity.FileName,
                Status = entity.Status,
                WordCount = entity.WordCount,
                LineCount = entity.LineCount,
                CharacterCount = entity.CharacterCount,
                MostFrequentWords = entity.MostFrequentWords,
                Summary = entity.Summary,
                ErrorMessage = entity.ErrorMessage,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static ProcessSagaStateDTO ToDto(ProcessSagaState entity)
        {
            if (entity == null) return null!;

            return new ProcessSagaStateDTO
            {
                CorrelationId = entity.CorrelationId,
                ProcessId = entity.ProcessId,
                CurrentState = entity.CurrentState,
                TotalFiles = entity.TotalFiles,
                CompletedFiles = entity.CompletedFiles,
                FailedFiles = entity.FailedFiles,
                SkippedFiles = entity.SkippedFiles,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
