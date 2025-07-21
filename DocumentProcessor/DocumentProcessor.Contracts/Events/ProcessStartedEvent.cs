using DocumentProcessor.Contracts.Base;

namespace DocumentProcessor.Contracts.Events
{
    public record ProcessStartedEvent : BaseProcessMessage
    {
        public List<string> FileNames { get; init; } = new();
    }
}
