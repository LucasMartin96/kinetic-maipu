namespace DocumentProcessor.Worker.Interfaces
{
    public interface ISummaryGenerator
    {
        string GenerateSummary(string content, int sentenceCount);
    }
} 