using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Commands
{
    public record UpdateFileStatusCommand : BaseFileMessage
    {
        public string NewStatus { get; init; } = string.Empty;
    }
} 