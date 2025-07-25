﻿using DocumentProcessor.Contracts;

namespace DocumentProcessor.Contracts.Dtos
{
    public class ProcessDTO
    {
        public Guid ProcessId { get; set; }
        public string Status { get; set; } = ProcessStatus.Pending;
        public int TotalFiles { get; set; }
        public int CompletedFiles { get; set; }
        public int FailedFiles { get; set; }
        public int SkippedFiles { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FolderPath { get; set; } = string.Empty;
        public List<FileDTO> Files { get; set; } = new();
    }
}
