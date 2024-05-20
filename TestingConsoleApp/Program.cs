using ConsoleUtility;
using ConsoleUtility.Classess;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TestingConsoleApp {
    [Command("test", "Test Command")]
    [ArgumentsDetail("NONE", "Test Description", 1)]
    [ArgumentsDetail("-test", "Test Description", 1)]
    class TestCommand : ICommand {
        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            UConsole.WriteLine($"NONE: {args["NONE", 0] ?? "Nothing"}");
            UConsole.WriteLine($"-test: {args["-test", 0] ?? "Nothing"}");
        }
    }

    class TestCommand2 : ICommand {
        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            UConsole.WriteLine($"NONE: {args["NONE", 0] ?? "Nothing"}");
            UConsole.WriteLine($"-test: {args["-test", 0] ?? "Nothing"}");
        }
    }

    class TestingConsoleApp {

        public static void Main(string[] args) {
            CommandManager.Initialized();
            CommandManager.AddCommand(new TestCommand2(), "test2", "Command that added by the 'CommandManager.AddCommand()' method", "Test Category", new ArgumentsDetails()
                .AddFlagDetail("NONE", "Test description", 1)
                .AddFlagDetail("-test", "Test Description", 1));

            UConsole.WriteLine(JsonConvert.SerializeObject(CommandManager.Commands[0], Formatting.Indented));
            UConsole.WriteLine(JsonConvert.SerializeObject(CommandManager.ParseArguments(["testing", "testing2", "testing3", "-test", "testing", "testing2", "testing3", "-test2", "testing", "testing2", "testing3"], CommandManager.Commands[0]), Formatting.Indented));
            CommandManager.ExecuteCommand(args);
            //// URL of the file containing words
            //string url = "https://gist.githubusercontent.com/h3xx/1976236/raw/bbabb412261386673eff521dddbe1dc815373b1d/wiki-100k.txt";

            //// Download the file
            //string content = DownloadFile(url);

            //// Set custom colors
            //UConsole.WriteLine("From Hex", "#ff0000");
            //UConsole.WriteLine("From Color", Color.Tan);
            //UConsole.WriteLine("From Console Color", ConsoleColor.Blue);
            //UConsole.WriteLineRichText("\n[u1][i1][red]This text is red\n[green]This text is green\n[blue]This text is blue");
            //UConsole.WriteLine();
            ////UConsole.ReadLineRichText("[u1][lightgreen]Type Here:[u0] ");
            //Stopwatch timer = new Stopwatch();
            //timer.Start();
            //List<string> suggestions = ExtractWords(content);

            //timer.Stop();

            //UConsole.WriteLine($"{suggestions.Count} words. Taken {(int)timer.Elapsed.TotalMilliseconds}ms");

            //UConsole.ReadKeyRichText(true, $"[#{UConsole.GetHexFromConsoleColor(ConsoleColor.Yellow)}]Press any key to continue.");
            //UConsole.Clear();
            //while(true) {
            //    string UserTypeText = UConsole.ReadLineWithSuggestions("Text Here: ", suggestions);
            //    UConsole.WriteLine($"You typed: {UserTypeText}");
            //    if (!suggestions.Contains(UserTypeText)) {
            //        suggestions.Insert(0, UserTypeText);
            //    }
            //    UConsole.WriteLine($"{suggestions.Count} words");
            //}
        }

        /// <summary>
        /// Function to download file content from URL
        /// </summary>
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


        /// <summary>
        /// Function to extract words from text content
        /// </summary>
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