using System.Text.Json;
using DocumentProcessor.Worker.Interfaces;
namespace DocumentProcessor.Worker.Services
{
    public class StopWordsFilter : IStopWordsFilter
    {
        private readonly ILogger<StopWordsFilter> _logger;

        public StopWordsFilter(ILogger<StopWordsFilter> logger)
        {
            _logger = logger;
        }

        public async Task<HashSet<string>> LoadStopWordsAsync()
        {
            var stopWords = new HashSet<string>();
            
            var possiblePaths = new[]
            {
                Path.Combine("/app/config/stopwords.json")
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        _logger.LogInformation("WORKER: Found stop words file at: {Path}", path);
                        var stopWordsJson = await File.ReadAllTextAsync(path);
                        var stopWordsList = JsonSerializer.Deserialize<List<string>>(stopWordsJson);
                        
                        if (stopWordsList != null)
                        {
                            stopWords = new HashSet<string>(stopWordsList, StringComparer.OrdinalIgnoreCase);
                            _logger.LogInformation("WORKER: Successfully loaded {Count} stop words from {Path}", stopWords.Count, path);
                            
                            _logger.LogInformation("WORKER: Key stop words check - 'de': {HasDe}, 'que': {HasQue}, 'no': {HasNo}, 'se': {HasSe}, 'del': {HasDel}", 
                                stopWords.Contains("de"), stopWords.Contains("que"), stopWords.Contains("no"), stopWords.Contains("se"), stopWords.Contains("del"));
                            
                            return stopWords;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("WORKER: Stop words file not found at: {Path}", path);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load stop words from {Path}", path);
                }
            }

            _logger.LogWarning("WORKER: No stop words file found, using default list");
            stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "el", "la", "que", "y", "a", "en", "un", "es", "se", "no", "te", "lo", "le", "da", "su", "por", "son", "con", "para", "al", "del", "los", "las", "una", "como", "pero", "sus", "me", "hasta", "hay", "donde", "han", "quien", "están", "estado", "desde", "todo", "nos", "durante", "todos", "podido", "tres", "tan", "así", "veinte", "año", "uno", "ya", "poco", "he", "esa", "poco", "esa", "en", "otro", "algunos", "algunas", "ser", "dos", "también", "era", "eran", "vez", "tienen", "esa", "porque", "muy", "sin", "sobre", "también", "mi", "contra", "solo", "han", "yo", "hay", "vez", "pueden", "todos", "así", "nos", "de"
            };

            return stopWords;
        }

        public bool IsStopWord(string word, HashSet<string> stopWords)
        {
            return stopWords.Contains(word);
        }
    }
} 