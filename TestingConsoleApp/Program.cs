using ConsoleUtility;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TestingConsoleApp {
    class TestingConsoleApp {
        public static void Main(string[] args) {
            // URL of the file containing words
            string url = "https://gist.githubusercontent.com/h3xx/1976236/raw/bbabb412261386673eff521dddbe1dc815373b1d/wiki-100k.txt";

            // Download the file
            string content = DownloadFile(url);

            // Set custom colors
            UConsole.WriteLine("From Hex", "#ff0000");
            UConsole.WriteLine("From Color", Color.Tan);
            UConsole.WriteLine("From Console Color", ConsoleColor.Blue);
            UConsole.WriteLineRichText("\n[u1][i1][red]This text is red\n[green]This text is green\n[blue]This text is blue");
            UConsole.WriteLine();
            //UConsole.ReadLineRichText("[u1][lightgreen]Type Here:[u0] ");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            List<string> suggestions = ExtractWords(content);

            timer.Stop();

            UConsole.WriteLine($"{suggestions.Count} words. Taken {(int)timer.Elapsed.TotalMilliseconds}ms");

            UConsole.ReadKeyRichText(true, $"[#{UConsole.GetHexFromConsoleColor(ConsoleColor.Yellow)}]Press any key to continue.");



            string currentSuggestions = "";
            UConsole.Clear();
            while(true) {
                UConsole.ReadLineWithSuggestionHandler("Text Here: ", text => {
                    if(text.Split(' ').Last().Length == 0) return new List<string>();

                    List<string> substringSuggestions = new List<string>();

                    List<string> currentSuggestions = suggestions.FindAll(suggestion => suggestion.StartsWith(text.ToLower().Split(' ').Last())) ?? new List<string>();
                    foreach(string currentSuggestion in currentSuggestions) {
                        int startIndex = Math.Min(text.Split(' ').Last().Length, currentSuggestion.Length); // Ensure start index doesn't exceed "Hello" length
                        int length = Math.Max(0, currentSuggestion.Length - text.Split(' ').Last().Length); // Calculate length of remaining portion of "Hello"
                        substringSuggestions.Add(currentSuggestion.Substring(startIndex, length));
                    }
                    return substringSuggestions;
                });
                UConsole.WriteLine();
                
            }

        }

        // Function to download file content from URL;

        static string DownloadFile(string url) {
            StringBuilder content = new StringBuilder();

            using(WebClient client = new WebClient()) {
                using(Stream stream = client.OpenRead(url)) {
                    using(StreamReader reader = new StreamReader(stream)) {
                        string line;
                        while((line = reader.ReadLine()) != null) {
                            if(!line.StartsWith("#!comment:")) {
                                content.AppendLine(line.ToLower());
                            }
                        }
                    }
                }
            }

            return content.ToString();
        }


        // Function to extract words from text content
        static List<string> ExtractWords(string text) {
            // Use regular expression to find words (non-empty sequences of letters)
            MatchCollection matches = Regex.Matches(text, @"\b[A-Za-z]+\b");

            // Extract words from matches
            List<string> words = new List<string>();
            foreach(Match match in matches) {
                if(words.Contains(match.Value.ToLower())) continue;
                words.Add(match.Value.ToLower()); // Convert words to lowercase
            }
            
            return words;
        }
    }


}