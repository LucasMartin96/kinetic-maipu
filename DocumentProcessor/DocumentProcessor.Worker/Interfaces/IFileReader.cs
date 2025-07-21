namespace DocumentProcessor.Worker.Interfaces
{
    public interface IFileReader
    {
        Task<string> ReadFileAsync(string filePath);
        bool FileExists(string filePath);
        long GetFileSize(string filePath);
    }
} 