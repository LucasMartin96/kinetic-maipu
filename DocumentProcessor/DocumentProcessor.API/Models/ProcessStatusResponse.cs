namespace DocumentProcessor.API.Models
{
    public class ProcessStatusResponse
    {
        public Guid ProcessId { get; set; }
        public string Status { get; set; } = string.Empty;
        public ProgressInfo Progress { get; set; } = new();
        public DateTime StartedAt { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
    }

    public class ProgressInfo
    {
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
        public int Percentage { get; set; }
    }
} 