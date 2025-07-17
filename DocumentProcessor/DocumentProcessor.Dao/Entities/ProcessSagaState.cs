namespace DocumentProcessor.Dao.Entities;

public class ProcessSagaState
{
    public Guid CorrelationId { get; set; }

    public Guid ProcessId { get; set; }

    public string CurrentState { get; set; } = string.Empty;

    public int TotalFiles { get; set; }
    public int CompletedFiles { get; set; }
    public int FailedFiles { get; set; }
    public int SkippedFiles { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
