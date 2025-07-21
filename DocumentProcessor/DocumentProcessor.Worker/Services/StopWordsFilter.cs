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
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a", "acá", "ahí", "al", "algo", "algunas", "alguno", "algunos", "allá", "allí", "ambos", "ante", "antes",
                "aquel", "aquella", "aquellas", "aquello", "aquellos", "aquí", "así", "aun", "aunque", "bajo", "bien", "cabe",
                "cada", "casi", "cierta", "ciertas", "cierto", "ciertos", "como", "con", "conmigo", "conseguimos", "conseguir",
                "consigo", "consigue", "consiguen", "consigues", "contigo", "contra", "cual", "cuales", "cualquier", "cualquiera",
                "cualquieras", "cuan", "cuando", "cuanta", "cuantas", "cuanto", "cuantos", "de", "dejar", "del", "demás", "demasiada",
                "demasiadas", "demasiado", "demasiados", "dentro", "desde", "donde", "dos", "el", "él", "ella", "ellas", "ello",
                "ellos", "empleais", "emplean", "emplear", "empleas", "empleo", "en", "encima", "entonces", "entre", "era", "erais",
                "eramos", "eran", "eras", "eres", "es", "esa", "esas", "ese", "eso", "esos", "esta", "estaba", "estabais", "estaban",
                "estabas", "estad", "estada", "estadas", "estado", "estados", "estais", "estamos", "estan", "estando", "estar",
                "estaremos", "estará", "estarán", "estarás", "estaré", "estaréis", "estaríamos", "estarían", "estarías", "estas",
                "este", "estemos", "estén", "estés", "esto", "estos", "estoy", "estuve", "estuviera", "estuvierais", "estuvieran",
                "estuvieras", "estuvieron", "estuviese", "estuvieseis", "estuviesen", "estuvieses", "estuvimos", "estuviste",
                "estuvisteis", "estuviéramos", "estuviésemos", "estuvo", "ex", "excepto", "fue", "fuera", "fuerais", "fueran",
                "fueras", "fueron", "fuese", "fueseis", "fuesen", "fueses", "fui", "fuimos", "fuiste", "fuisteis", "gran", "grandes",
                "gueno", "ha", "habéis", "había", "habíais", "habíamos", "habían", "habías", "haber", "habremos", "habrá", "habrán",
                "habrás", "habré", "habréis", "habría", "habríais", "habríamos", "habrían", "habrías", "habéis", "habíamos", "había",
                "hace", "haceis", "hacemos", "hacen", "hacer", "haces", "hacia", "hago", "han", "has", "hasta", "hay", "haya", "hayamos",
                "hayan", "hayas", "he", "hemos", "hicieron", "hizo", "hoy", "hubiera", "hubierais", "hubieran", "hubieras", "hubieron",
                "hubiese", "hubieseis", "hubiesen", "hubieses", "hubimos", "hubiste", "hubisteis", "hubiéramos", "hubiésemos", "hubo",
                "igual", "incluso", "intenta", "intentais", "intentamos", "intentan", "intentar", "intentas", "intento", "ir", "jamás",
                "junto", "juntos", "la", "largo", "las", "le", "les", "lo", "los", "luego", "mal", "más", "me", "menos", "mi", "mía",
                "mías", "mientras", "mío", "míos", "mis", "misma", "mismas", "mismo", "mismos", "modo", "mucha", "muchas", "muchísima",
                "muchísimas", "muchísimo", "muchísimos", "mucho", "muchos", "muy", "nada", "ni", "ninguna", "ningunas", "ninguno",
                "ningunos", "no", "nos", "nosotras", "nosotros", "nuestra", "nuestras", "nuestro", "nuestros", "nunca", "o", "os",
                "otra", "otras", "otro", "otros", "para", "parece", "parte", "pero", "poca", "pocas", "poco", "pocos", "podemos",
                "poder", "podrá", "podrán", "podría", "podrían", "poner", "por", "por qué", "porque", "primero", "puede", "pueden",
                "puedo", "pues", "que", "qué", "querer", "quien", "quienes", "quiere", "quién", "quiénes", "quizá", "quizás", "sabe",
                "sabes", "salvo", "se", "sea", "seamos", "sean", "seas", "según", "ser", "seremos", "será", "serán", "serás", "seré",
                "seréis", "sería", "seríais", "seríamos", "serían", "serías", "si", "sí", "sido", "siendo", "sin", "sino", "so", "sobre",
                "sois", "solamente", "solo", "somos", "soy", "su", "sus", "suya", "suyas", "suyo", "suyos", "tal", "tales", "también",
                "tampoco", "tan", "tanta", "tantas", "tanto", "tantos", "te", "tendrá", "tendrán", "tendré", "tendréis", "tendría",
                "tendríais", "tendríamos", "tendrían", "tendrías", "tened", "tenemos", "tener", "tenga", "tengamos", "tengan", "tengas",
                "tengo", "tenida", "tenidas", "tenido", "tenidos", "tenéis", "tenía", "teníais", "teníamos", "tenían", "tenías", "ti",
                "tiempo", "tiene", "tienen", "tienes", "toda", "todas", "todavía", "todo", "todos", "tomar", "trabaja", "trabajais",
                "trabajamos", "trabajan", "trabajar", "trabajas", "trabajo", "tras", "tú", "tu", "tus", "tuve", "tuviera", "tuvierais",
                "tuvieran", "tuvieras", "tuvieron", "tuviese", "tuvieseis", "tuviesen", "tuvieses", "tuvimos", "tuviste", "tuvisteis",
                "tuviéramos", "tuviésemos", "tuvo", "tuya", "tuyas", "tuyo", "tuyos", "un", "una", "unas", "uno", "unos", "usted",
                "ustedes", "va", "vais", "valor", "vamos", "van", "varias", "varios", "vaya", "veces", "ver", "verdad", "verdadera",
                "verdadero", "vez", "vosotras", "vosotros", "voy", "vuestra", "vuestras", "vuestro", "vuestros", "y", "ya", "yo"
            };

            // Por si las dudas...
            var gutenbergHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "project", "gutenberg", "ebook", "e-book", "this", "book", "is", "for", "the", "use", "of", "anyone", "anywhere",
                "united", "states", "most", "other", "parts", "world", "at", "no", "cost", "and", "with", "almost", "restrictions",
                "whatsoever", "you", "may", "copy", "it", "give", "away", "re-use", "under", "terms", "license", "included", "online",
                "www.gutenberg.org", "located", "check", "laws", "country", "before", "using", "title", "author", "translator",
                "release", "date", "language", "credits", "university", "toronto", "internet", "archive", "distributed",
                "proofreading", "team", "produced", "images", "made", "available", "hathitrust", "digital", "library",
                "start", "end", "of", "the", "project", "gutenberg", "ebook", "el", "crimen", "y", "castigo"
            };

            stopWords.UnionWith(gutenbergHeaders);

            return stopWords;
        }

        public bool IsStopWord(string word, HashSet<string> stopWords)
        {
            return stopWords.Contains(word);
        }
    }
} 