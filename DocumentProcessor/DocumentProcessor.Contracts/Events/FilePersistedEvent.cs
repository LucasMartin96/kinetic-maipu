using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Events
{
    public record FilePersistedEvent : BaseFileMessage
    {
        public string Status { get; init; } = string.Empty;
    }
} 