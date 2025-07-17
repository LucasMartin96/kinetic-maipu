namespace DocumentProcessor.Dao.Entities;
public class Process
{
    public Guid ProcessId { get; set; }

    public string Status { get; set; } = "PENDING";

    public int TotalFiles { get; set; }
    public int CompletedFiles { get; set; } = 0;
    public int FailedFiles { get; set; } = 0;
    public int SkippedFiles { get; set; } = 0;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<File> Files { get; set; } = new List<File>();
}