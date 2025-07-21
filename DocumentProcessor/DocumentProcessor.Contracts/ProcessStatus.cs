namespace DocumentProcessor.Contracts
{
    public static class ProcessStatus
    {
        public const string Pending = "PENDING";
        public const string Running = "RUNNING";
        public const string Completed = "COMPLETED";
        public const string Failed = "FAILED";
        public const string Stopped = "STOPPED";
        public const string CompletedWithFailures = "COMPLETED_WITH_FAILURES";
    }
} 