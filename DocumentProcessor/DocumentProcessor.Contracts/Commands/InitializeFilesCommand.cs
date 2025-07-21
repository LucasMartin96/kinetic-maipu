using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Commands
{
    public record InitializeFilesCommand : BaseProcessMessage
    {
        public List<string> FileNames { get; init; } = new();
    }
}
