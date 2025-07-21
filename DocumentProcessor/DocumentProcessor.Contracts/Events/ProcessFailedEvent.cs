using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Events
{
    public record ProcessFailedEvent : BaseProcessMessage
    {
        public string Reason { get; init; } = string.Empty;
    }
} 