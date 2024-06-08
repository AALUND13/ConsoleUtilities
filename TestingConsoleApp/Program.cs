using AALUND13.ConsoleUtility;
using AALUND13.ConsoleUtility.Classess;
using System.Drawing;

namespace TestingConsoleApp {
    [Command("test-color", "Write some color test to the console.", "test")]
    public class ConsoleColorTest : ICommand {
        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            UConsole.WriteLine("Testing Non Rich Text");
            UConsole.WriteLine("From Hex", "#ff0000");
            UConsole.WriteLine("From Color", Color.Tan);
            UConsole.WriteLine("From Console Color", ConsoleColor.Blue);
            UConsole.WriteLine();
            UConsole.WriteLine("Testing Rich Text");
            UConsole.WriteLineRichText("[u1][i1][red]This text is red\n[green]This text is green\n[blue]This text is blue");
        }
    }

    [Command("test-color", "Write some color test to the console.", "test")]
    public class ConsoleColor2Test : ICommand {
        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            UConsole.WriteLine("Testing Non Rich Text");
            UConsole.WriteLine("From Hex", "#ff0000");
            UConsole.WriteLine("From Color", Color.Tan);
            UConsole.WriteLine("From Console Color", ConsoleColor.Blue);

            UConsole.WriteLine();
            UConsole.WriteLine("Testing Rich Text");
            UConsole.WriteLineRichText("[u1][i1][red]This text is red\n[green]This text is green\n[blue]This text is blue");
        }
    }


    [Command("test-suggestions", "Test the suggestion feature", "test")]
    [ArgumentsDetail("-s", "List of suggestions")]
    public class TestSuggestionsHandlerCommand : ICommand {
        public List<string> DefaultSuggestions = new List<string>() { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };
        public List<string> Suggestions = new List<string>();

        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            if (args["-s"].Count > 0)
                Suggestions = args["-s"];
            else
                Suggestions = DefaultSuggestions;

            string word = UConsole.ReadLineWithSuggestions("Type Here: ", Suggestions);
            UConsole.WriteLine($"You typed: {word}");
        }
    }

    class TestingConsoleApp {
        public static void Main(string[] args) {
            CommandManager.ExecuteCommand(args);
        }
    }


}