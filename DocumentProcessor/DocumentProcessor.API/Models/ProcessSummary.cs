namespace DocumentProcessor.API.Models
{
    public class ProcessSummary
    {
        public Guid ProcessId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalFiles { get; set; }
        public int CompletedFiles { get; set; }
        public int FailedFiles { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 