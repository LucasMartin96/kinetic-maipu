using DocumentProcessor.Contracts.Dtos;
using DocumentProcessor.Dao.Entities;
namespace DocumentProcessor.Dao.Mappers
{
    public static class DtoToEntityMapper
    {
        public static Process ToEntity(ProcessDTO dto)
        {
            if (dto == null) return null!;

            return new Process
            {
                ProcessId = dto.ProcessId,
                Status = dto.Status,
                TotalFiles = dto.TotalFiles,
                CompletedFiles = dto.CompletedFiles,
                FailedFiles = dto.FailedFiles,
                SkippedFiles = dto.SkippedFiles,
                FolderPath = dto.FolderPath,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt,
                Files = dto.Files?.Select(f => ToEntity(f)).ToList() ?? new List<Entities.File>()
            };
        }

        public static Entities.File ToEntity(FileDTO dto)
        {
            if (dto == null) return null!;

            return new Entities.File
            {
                FileId = dto.FileId,
                ProcessId = dto.ProcessId,
                FileName = dto.FileName,
                Status = dto.Status,
                WordCount = dto.WordCount,
                LineCount = dto.LineCount,
                CharacterCount = dto.CharacterCount,
                MostFrequentWords = dto.MostFrequentWords,
                Summary = dto.Summary,
                ErrorMessage = dto.ErrorMessage,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        public static ProcessSagaState ToEntity(ProcessSagaStateDTO dto)
        {
            if (dto == null) return null!;

            return new ProcessSagaState
            {
                CorrelationId = dto.CorrelationId,
                ProcessId = dto.ProcessId,
                CurrentState = dto.CurrentState,
                TotalFiles = dto.TotalFiles,
                CompletedFiles = dto.CompletedFiles,
                FailedFiles = dto.FailedFiles,
                SkippedFiles = dto.SkippedFiles,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}
