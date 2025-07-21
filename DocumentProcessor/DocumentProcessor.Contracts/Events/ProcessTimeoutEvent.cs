namespace DocumentProcessor.Contracts.Events;

public record ProcessTimeoutEvent
{
    public Guid ProcessId { get; init; }
    public DateTime TimedOutAt { get; init; } = DateTime.UtcNow;
} 