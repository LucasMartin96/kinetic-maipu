using DocumentProcessor.Worker.Interfaces;
using DocumentProcessor.Worker.Interfaces;

namespace DocumentProcessor.Worker.Services
{
    public class WordFrequencyAnalyzer : IWordFrequencyAnalyzer
    {
        private readonly ILogger<WordFrequencyAnalyzer> _logger;

        public WordFrequencyAnalyzer(ILogger<WordFrequencyAnalyzer> logger)
        {
            _logger = logger;
        }

        public List<string> GetMostFrequentWords(List<string> words, int count)
        {
            var mostFrequentWords = words
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToList();

            _logger.LogInformation("WORKER: Most frequent words: [{Words}]", string.Join(", ", mostFrequentWords));

            return mostFrequentWords;
        }
    }
} 