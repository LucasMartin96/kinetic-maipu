using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Events
{
    public record FileFailedEvent : BaseFileMessage
    {
        public string ErrorMessage { get; init; } = string.Empty;
    }
}
