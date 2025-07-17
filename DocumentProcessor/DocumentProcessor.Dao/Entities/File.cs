namespace DocumentProcessor.Dao.Entities;
public class File
{
    public Guid FileId { get; set; }
    public Guid ProcessId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public int WordCount { get; set; }
    public int LineCount { get; set; }
    public int CharacterCount { get; set; }
    public string MostFrequentWords { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Process? Process { get; set; }
}