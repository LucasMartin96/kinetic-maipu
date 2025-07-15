namespace DocumentProcessor.Contracts.TestsRecords
{
    public record ProcessStartedTestEvent(Guid ProcessId);

    public record ProcessFilesTestCommand
    {
        public Guid ProcessId { get; init; }
        public string[] Files { get; init; }
    }
    public record ProcessFilesTestSlaveCommand
    {
        public Guid ProcessId { get; init; }
        public string[] Files { get; init; } = Array.Empty<string>();
    }

    public record FileProcessedTestEvent
    {
        public Guid ProcessId { get; init; }
        public string FileName { get; init; } = string.Empty;
    }
    public record ProcessCompletedTestEvent
    {
        public Guid ProcessId { get; init; }
    }
}
