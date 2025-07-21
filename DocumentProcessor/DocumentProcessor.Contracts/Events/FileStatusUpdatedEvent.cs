using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Events
{
    public record FileStatusUpdatedEvent : BaseFileMessage
    {
        public string NewStatus { get; init; } = string.Empty;
    }
} 