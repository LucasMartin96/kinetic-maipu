namespace DocumentProcessor.Worker.Models
{
    public class TextAnalysisResult
    {
        public int WordCount { get; set; }
        public int LineCount { get; set; }
        public int CharacterCount { get; set; }
        public string MostFrequentWords { get; set; } = string.Empty;
        public string[] MostFrequentWordsArray { get; set; } = Array.Empty<string>();
        public string Summary { get; set; } = string.Empty;
    }
} 