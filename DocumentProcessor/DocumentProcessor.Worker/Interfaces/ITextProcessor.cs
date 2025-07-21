using DocumentProcessor.Worker.Models;

namespace DocumentProcessor.Worker.Interfaces
{
    public interface ITextProcessor
    {
        Task<TextAnalysisResult> ProcessTextAsync(string content);
    }
} 