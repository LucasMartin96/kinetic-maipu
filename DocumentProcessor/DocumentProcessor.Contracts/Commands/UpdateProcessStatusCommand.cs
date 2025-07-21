using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Commands
{
    public record UpdateProcessStatusCommand : BaseProcessMessage
    {
        public string NewStatus { get; init; } = string.Empty;
        public string? Reason { get; init; }
    }
}
