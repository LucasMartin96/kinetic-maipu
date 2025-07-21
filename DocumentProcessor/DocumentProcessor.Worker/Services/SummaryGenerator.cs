using DocumentProcessor.Worker.Interfaces;

using DocumentProcessor.Worker.Interfaces;

namespace DocumentProcessor.Worker.Services
{
    public class SummaryGenerator : ISummaryGenerator
    {
        private readonly ILogger<SummaryGenerator> _logger;

        public SummaryGenerator(ILogger<SummaryGenerator> logger)
        {
            _logger = logger;
        }

        public string GenerateSummary(string content, int sentenceCount)
        {
            var sentences = content.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var summary = string.Join(". ", sentences.Take(sentenceCount)) + ".";

            _logger.LogInformation("WORKER: Generated summary with {SentenceCount} sentences", sentenceCount);

            return summary;
        }
    }
} 