using DocumentProcessor.Worker.Interfaces;
using System.Text;
using DocumentProcessor.Worker.Interfaces;

namespace DocumentProcessor.Worker.Services
{
    public class FileReader : IFileReader
    {
        private readonly ILogger<FileReader> _logger;

        public FileReader(ILogger<FileReader> logger)
        {
            _logger = logger;
        }

        public async Task<string> ReadFileAsync(string filePath)
        {
            _logger.LogInformation("WORKER: Reading file from: {FilePath}", filePath);
            
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty");
            }

            if (!FileExists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            try
            {
                var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                _logger.LogInformation("WORKER: File content loaded - Size: {Size} characters", content.Length);
                
                return content;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "WORKER: Access denied to file {FilePath}", filePath);
                throw new InvalidOperationException($"Access denied to file: {filePath}", ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "WORKER: IO error reading file {FilePath}", filePath);
                throw new InvalidOperationException($"IO error reading file: {filePath}", ex);
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public long GetFileSize(string filePath)
        {
            if (!FileExists(filePath))
                return 0;
            
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
    }
} 