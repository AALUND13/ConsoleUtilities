using ConsoleUtility.Classess;
using System.Reflection;

namespace ConsoleUtility {
    public static class CommandManager {
        public static List<Command> Commands { get; private set; } = new List<Command>();

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
    



    }
}
