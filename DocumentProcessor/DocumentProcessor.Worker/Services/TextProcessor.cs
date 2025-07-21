using DocumentProcessor.Worker.Interfaces;
using DocumentProcessor.Worker.Models;
using DocumentProcessor.Worker.Interfaces;

namespace DocumentProcessor.Worker.Services
{
    public class TextProcessor : ITextProcessor
    {
        private readonly IStopWordsFilter _stopWordsFilter;
        private readonly IWordFrequencyAnalyzer _wordAnalyzer;
        private readonly ISummaryGenerator _summaryGenerator;
        private readonly ILogger<TextProcessor> _logger;

        public TextProcessor(
            IStopWordsFilter stopWordsFilter,
            IWordFrequencyAnalyzer wordAnalyzer,
            ISummaryGenerator summaryGenerator,
            ILogger<TextProcessor> logger)
        {
            _stopWordsFilter = stopWordsFilter;
            _wordAnalyzer = wordAnalyzer;
            _summaryGenerator = summaryGenerator;
            _logger = logger;
        }

        public async Task<TextAnalysisResult> ProcessTextAsync(string content)
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            var rawWords = content.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\'', '-', '_', '=', '+', '*', '/', '\\', '|', '&', '^', '%', '$', '#', '@', '!', '¡', '¿' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            var words = rawWords
                .Where(w => w.Length > 1) 
                .Select(w => w.Trim().ToLowerInvariant()) 
                .Where(w => !string.IsNullOrWhiteSpace(w)) 
                .ToList();

            var stopWords = await _stopWordsFilter.LoadStopWordsAsync();
            var filteredWords = words.Where(w => !_stopWordsFilter.IsStopWord(w, stopWords)).ToList();

            var wordCount = filteredWords.Count;
            var lineCount = lines.Length;
            var characterCount = content.Length;

            var mostFrequentWords = _wordAnalyzer.GetMostFrequentWords(filteredWords, 10);
            var summary = _summaryGenerator.GenerateSummary(content, 3);

            _logger.LogInformation("WORKER: Processed text - Words: {WordCount}, Lines: {LineCount}, Characters: {CharCount}", 
                wordCount, lineCount, characterCount);

            return new TextAnalysisResult
            {
                WordCount = wordCount,
                LineCount = lineCount,
                CharacterCount = characterCount,
                MostFrequentWords = string.Join(",", mostFrequentWords),
                MostFrequentWordsArray = mostFrequentWords.ToArray(),
                Summary = summary
            };
        }
    }
} 