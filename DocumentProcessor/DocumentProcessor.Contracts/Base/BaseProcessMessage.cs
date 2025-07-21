namespace DocumentProcessor.Contracts.Base
{
    public record BaseProcessMessage
    {
        public Guid ProcessId { get; init; }
    }
}
