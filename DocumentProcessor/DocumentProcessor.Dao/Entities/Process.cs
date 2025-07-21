using System.ComponentModel.DataAnnotations;

namespace DocumentProcessor.Dao.Entities;
public class Process
{
    public Guid ProcessId { get; set; }


    [MaxLength(64)]
    public string Status { get; set; } = "PENDING";
    public int TotalFiles { get; set; }
    public int CompletedFiles { get; set; } = 0;
    public int FailedFiles { get; set; } = 0;
    public int SkippedFiles { get; set; } = 0;
    public string FolderPath { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<File> Files { get; set; } = new List<File>();
}