namespace DocumentProcessor.Worker.Interfaces
{
    public interface IWordFrequencyAnalyzer
    {
        List<string> GetMostFrequentWords(List<string> words, int count);
    }
} 