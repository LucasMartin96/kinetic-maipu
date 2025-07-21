namespace DocumentProcessor.Contracts.Dtos
{
    public class FileDTO
    {
        public Guid FileId { get; set; }
        public Guid ProcessId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int WordCount { get; set; }
        public int LineCount { get; set; }
        public int CharacterCount { get; set; }
        public string MostFrequentWords { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
