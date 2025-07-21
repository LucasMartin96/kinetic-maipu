namespace DocumentProcessor.API.Models
{
    public class ProcessStartRequest
    {
        public List<IFormFile> Files { get; set; } = new();
    }
} 