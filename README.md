# ConsoleUtility

ConsoleUtility is an easy-to-use library for creating console applications in C#.

## Installation

You can install ConsoleUtility via NuGet Package Manager Console by running the following command:

```bash
dotnet add package AALUND13.ConsoleUtilities
```

Or by adding the following line to your `.csproj` file:

```xml
<PackageReference Include="AALUND13.ConsoleUtilities" Version="1.0.1" />
```

## Features

- Command Handler / Command Suggestions
- Suggestions Handler
- Better Console Class Called `UConsole`
- Easy To Use

## Example

### Command Handler Example
```csharp
using AALUND13.ConsoleUtility;
using AALUND13.ConsoleUtility.Classess;

// Create a command called `test` in the `test` category that will print "This is a test command" in hex color `#FF0000`. 
// This command will be initialized automatically.
[Command("test", "This is a test command", "test")]
class TestCommand : ICommand {
    public void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly) {
        UConsole.WriteLine("This is a test command", "#FF0000");
    }
}

class Program {
    static void Main(string[] args) {
        CommandManager.ExecuteCommand(args);
    }
}
```

### UConsole Example

```csharp
using AALUND13.ConsoleUtility;

class Program {
    static void Main(string[] args) {
        // Write a message to the console with the hex color #FF0000
        UConsole.WriteLine("This is a test message", "#FF0000");
        
        // Fully clear the console unlike Console.Clear() that does not remove the text above the console
        UConsole.Clear();
        
        // Write a rich text message to the console with the hex color #FF0000
        UConsole.WriteLineRichText("[#FF0000]This is a rich test message with hex color #FF0000");
    }
}
```

### Suggestions Handler Example

```csharp
using AALUND13.ConsoleUtility;

class Program {
    static void Main(string[] args) {
        // There are two methods to add suggestions:
        // 1. `public static string ReadLineWithSuggestions(string richTextValue, List<string> suggestions, bool ignoreCase = true)`
        //    This method takes a list of strings and returns the string that the user typed.
        // 2. `public static string ReadLineWithSuggestions(string richTextValue, Func<string, List<string>> handler)`
        //    This method takes a function that returns a list of strings and returns the string that the user typed.
        
        // Using the first method in this example:
        List<string> suggestions = new List<string>() { "red", "orange", "yellow", "green", "blue", "indigo", "violet" };

        string word = UConsole.ReadLineWithSuggestions("Type Here: ", suggestions);
        UConsole.WriteLine($"You typed: {word}");
    }
}
```

By utilizing ConsoleUtility, you can greatly enhance the functionality and user experience of your console applications with features like command handling, suggestion prompts, and extended console color support.