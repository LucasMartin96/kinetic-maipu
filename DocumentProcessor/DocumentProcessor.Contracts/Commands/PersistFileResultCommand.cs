using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Commands
{
    public record PersistFileResultCommand : BaseFileMessage
    {
        public string Status { get; init; } = string.Empty;
        public int WordCount { get; init; }
        public int LineCount { get; init; }
        public int CharacterCount { get; init; }
        public string[] MostFrequentWords { get; init; } = Array.Empty<string>();
        public string Summary { get; init; } = string.Empty;
        public string ErrorMessage { get; init; } = string.Empty;
    }
}
