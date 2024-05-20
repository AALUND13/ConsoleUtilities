using ConsoleUtility.Classess;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ConsoleUtility {
    /* TODO:
     * 
     * Add built-in commands (help, exit, clear).
     * 
     * Add a way to manually add commands.
     * Add a way to remove commands.
     */

    [Command("exit", "Exits the command prompt.")]
    public class ExitCommand : ICommand {
        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            CommandManager.CommandMode = false;
        }
    }

    [Command("clear", "Clears the console.")]
    public class ClearCommand : ICommand {
        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            UConsole.Clear();
        }
    }

    [Command("help", "Displays the list of commands.")]
    [ArgumentsDetail("-c", "Displays the command description.", 1)]
    public class HelpCommand : ICommand {
        public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
            if(args["-c", 0] != null) {
                Command command = CommandManager.Commands.Find(c => c.CommandName == args["-c", 0]);
                if (command != null) {
                    UConsole.WriteLine($"Command: {command.CommandName}\nDescription: {command.CommandDescription}\n\nArguments:");

                    command.CommandArgsDetail.Foreach((flagName, flagDescription, flagCapacity) => {
                        UConsole.WriteLine($"Flag: {flagName}\nDescription: {flagDescription}\nCapacity: {flagCapacity}");
                    });
                } else {
                    UConsole.WriteLine($"Command '{args["-c", 0]}' not found.", ConsoleColor.Red);
                }
            } else {
                foreach (Command command in CommandManager.Commands) {
                    UConsole.WriteLine($"{command.CommandName} - {command.CommandDescription}");
                }
            }
            UConsole.WriteLine();
        }
    }

    public static class CommandManager {
        public static List<Command> Commands { get; private set; } = new List<Command>();
        public static bool CommandMode = true;

        /// <summary>
        /// {path} is where the command being executed
        /// </summary>
        public static string CommandPromptText = ">> ";

        private static List<string> _history = new List<string>();

        public static void Initialized() {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly assembly in assemblies) {
                IEnumerable<Type> commandTypes = assembly.GetTypes().Where(type =>
                    type.GetInterfaces().Contains(typeof(ICommand)) &&
                    type.GetCustomAttribute<CommandAttribute>() != null);

                foreach(Type commandType in commandTypes) {
                    ICommand commandInstance = (ICommand)Activator.CreateInstance(commandType);
                    CommandAttribute commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();
                    IEnumerable<ArgumentsDetailAttribute> argumentsDetailAttributes = commandType.GetCustomAttributes<ArgumentsDetailAttribute>();

                    // Create a new instance of ArgumentsDetails for each command
                    ArgumentsDetails argumentsDetails = new ArgumentsDetails();

                    foreach(var attribute in argumentsDetailAttributes) {
                        argumentsDetails = argumentsDetails.AddFlagDetail(attribute.FlagName, attribute.FlagDescription, attribute.FlagParameterCapacity);
                    }

                    Command command = new Command {
                        CommandName = commandAttribute.CommandName,
                        CommandDescription = commandAttribute.CommandDescription,
                        CommandArgsDetail = argumentsDetails,
                        CommandImplementation = commandInstance
                    };

                    Commands.Add(command);
                }
            }
        }

        public static Arguments ParseArguments(string[] args, Command command) {
            Arguments arguments = new Arguments(command.CommandArgsDetail);

            string currentFlag = "NONE";
            int currentFlagIndex = command.CommandArgsDetail.FlagParameterCapacity["NONE"] == 0 ? int.MaxValue :
                        command.CommandArgsDetail.FlagParameterCapacity["NONE"];

            for(int i = 0; i < args.Length; i++) {
                if(command.CommandArgsDetail.FlagNames.Contains(args[i])) {
                    currentFlag = args[i];
                    currentFlagIndex = command.CommandArgsDetail.FlagParameterCapacity[currentFlag] == 0 ? int.MaxValue :
                        command.CommandArgsDetail.FlagParameterCapacity[currentFlag] - arguments.FlagParameterPairs[currentFlag].Count;

                } else {
                    if(currentFlagIndex == 0 && currentFlag != "NONE") {
                        currentFlag = "NONE";
                        currentFlagIndex = command.CommandArgsDetail.FlagParameterCapacity["NONE"] == 0 ? int.MaxValue :
                        command.CommandArgsDetail.FlagParameterCapacity["NONE"] - arguments.FlagParameterPairs["NONE"].Count;
                    }

                    if(currentFlagIndex == 0 && currentFlag == "NONE") continue;

                    arguments.AddParameterToFlag(currentFlag, args[i]);
                    currentFlagIndex--;
                }
            }

            return arguments;
        }

        public static (string[], string[]) ParseArguments(string input) {
            Regex regex = new Regex(@"""[^""]+""|[^ ]+", RegexOptions.Compiled);
            MatchCollection matches = regex.Matches(input);
            List<string> args = new List<string>();
            List<string> argsWithoutFormatting = new List<string>();

            foreach(Match match in matches) {
                string arg = match.Value;
                if(arg.StartsWith("\"") && arg.EndsWith("\"") && arg.Length >= 2) {
                    // Remove quotes and add the argument
                    args.Add(arg.Substring(1, arg.Length - 2));
                } else {
                    args.Add(arg);
                }
                argsWithoutFormatting.Add(arg);
            }

            return (args.ToArray(), argsWithoutFormatting.ToArray());
        }

        private static List<string> CommandsSuggestionsHandler(string text) {
            List<string> substringSuggestions = new List<string>();

            // Find all suggestions that start with the last word in the input text
            // if ignoreCase is true, ignore case
            (string[] arguments, string[] argumentsWithoutFormatting) = ParseArguments(text);

            List<string> currentSuggestions = new List<string>(_history.FindAll(history => history.StartsWith(text)));

            if(argumentsWithoutFormatting.Length == 1 && !text.EndsWith(' ')) currentSuggestions.AddRange(Commands.Select(c => c.CommandName).Where(c => c.StartsWith(argumentsWithoutFormatting[0])).ToList());
            else if(argumentsWithoutFormatting.Length > 1 && !text.EndsWith("\"\"")) {
                Command command = Commands.Find(c => c.CommandName == argumentsWithoutFormatting[0].Trim());
                if(command != null) {
                    currentSuggestions.AddRange(command.CommandArgsDetail.FlagNames.Where(flag => flag != "NONE" && flag.StartsWith(argumentsWithoutFormatting.Last())).ToList());
                }
            }


            Regex regex = new Regex(@"""[^""]*""\s*|""[^""]*|[^ ]+\s*", RegexOptions.Compiled);
            MatchCollection matches = regex.Matches(text);
            List<string> args = new List<string>();

            foreach(string currentSuggestion in currentSuggestions) {
                if(matches.Count != 0 && !_history.Any(history => history.StartsWith(currentSuggestion))) {
                    int startIndex = Math.Min(matches.Last().Value.Length, currentSuggestion.Length); // Ensure start index doesn't exceed currentSuggestion length
                    int length = Math.Max(0, currentSuggestion.Length - matches.Last().Value.Length); // Calculate length of remaining portion of currentSuggestion
                    substringSuggestions.Add(currentSuggestion.Substring(startIndex, length));
                } else {
                    int startIndex = Math.Min(text.Length, currentSuggestion.Length); // Ensure start index doesn't exceed currentSuggestion length
                    int length = Math.Max(0, currentSuggestion.Length - text.Length); // Calculate length of remaining portion of currentSuggestion
                    if(!substringSuggestions.Contains(currentSuggestion.Substring(startIndex, length))) substringSuggestions.Add(currentSuggestion.Substring(startIndex, length));
                }
            }
            return substringSuggestions;
        }

        public static bool ExecuteCommand(string[] args, bool runOnes = false) {
            if(args.Length > 0 && (runOnes || !string.IsNullOrWhiteSpace(args[0]))) {
                Command command = Commands.FirstOrDefault(c => c.CommandName == args[0]);
                if(command == null) {
                    UConsole.WriteLine($"Command '{args[0]}' not found. Use the \"help\" command to pull up the list of commands.", ConsoleColor.Red);
                    return false;
                }

                string[] commandArgs = args.Skip(1).ToArray();
                try {
                    command.CommandImplementation.OnExecute(ParseArguments(commandArgs, command), Environment.CurrentDirectory, true);
                    return true;
                } catch(Exception ex) {
                    UConsole.WriteLine($"Caught an unhandled exception: {ex}", ConsoleColor.Red);
                    return false;
                }
            } else {
                while(CommandMode) {
                    if(runOnes) CommandMode = false;

                    string? input = UConsole.ReadLineWithSuggestions(CommandPromptText.Replace("{path}", Environment.CurrentDirectory), CommandsSuggestionsHandler) ?? "";
                    (string[] arguments, string[] argumentsWithoutFormatting) = ParseArguments(input);

                    if(arguments.Length == 0) continue;

                    Command command = Commands.Find(c => c.CommandName == arguments[0]);
                    if(command == null) {
                        UConsole.WriteLine($"Command '{arguments[0]}' not found. Use the \"help\" command to pull up the list of commands.", ConsoleColor.Red);
                        continue;
                    }

                    Arguments cmd = ParseArguments(arguments.Skip(1).ToArray(), command);
                    try {
                        command.CommandImplementation.OnExecute(cmd, Environment.CurrentDirectory, false);

                        if(!_history.Contains(input)) {
                            _history.Insert(0, input);
                        } else {
                            _history.Remove(input);
                            _history.Insert(0, input);
                        }
                    } catch(Exception ex) {
                        UConsole.WriteLine($"Caught an unhandled exception: {ex}", ConsoleColor.Red);
                    }
                }
            }

            return true;
        }
    }
}
