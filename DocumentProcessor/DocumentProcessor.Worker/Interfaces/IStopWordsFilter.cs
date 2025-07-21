namespace DocumentProcessor.Worker.Interfaces
{
    public interface IStopWordsFilter
    {
        Task<HashSet<string>> LoadStopWordsAsync();
        bool IsStopWord(string word, HashSet<string> stopWords);
    }
} 