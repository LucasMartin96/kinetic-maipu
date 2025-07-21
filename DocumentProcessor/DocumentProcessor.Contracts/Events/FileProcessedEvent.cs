using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Events
{
    public record FileProcessedEvent : BaseFileMessage
    {
        public int WordCount { get; init; }
        public int LineCount { get; init; }
        public int CharacterCount { get; init; }
        public string[] MostFrequentWords { get; init; } = Array.Empty<string>();
        public string Summary { get; init; } = string.Empty;
    }
}
