namespace DocumentProcessor.API.Models
{
    public class ProcessResultsResponse
    {
        public Guid ProcessId { get; set; }
        public string Status { get; set; } = string.Empty;
        public ProcessResults Results { get; set; } = new();
    }

    public class ProcessResults
    {
        public int TotalWords { get; set; }
        public int TotalLines { get; set; }
        public List<string> MostFrequentWords { get; set; } = new();
        public List<string> FilesProcessed { get; set; } = new();
    }
} 