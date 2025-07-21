namespace DocumentProcessor.API.Models
{
    public class ProcessStartResult
    {
        public Guid ProcessId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 