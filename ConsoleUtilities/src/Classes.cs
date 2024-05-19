namespace ConsoleUtility.Classess {
    public struct ArgumentsDetails {
        public Dictionary<string, string> FlagDescription { get; private set; }
        public Dictionary<string, int> FlagParameterCapacity { get; private set; }
        public Dictionary<string, List<string>> FlagAutoComplete { get; set; }

        public List<string> FlagNames {
            get {
                return FlagDescription.Keys.ToList();
            }
        }

        public ArgumentsDetails(ArgumentsDetails argumentsDetail) {
            FlagParameterCapacity = argumentsDetail.FlagParameterCapacity;
            FlagDescription = argumentsDetail.FlagDescription;
            FlagAutoComplete = argumentsDetail.FlagAutoComplete;
        }

        public ArgumentsDetails() {
            FlagDescription = new Dictionary<string, string>();
            FlagParameterCapacity = new Dictionary<string, int>();
            FlagAutoComplete = new Dictionary<string, List<string>>();
            AddFlagDetail("NONE", "No description.");
        }

        public ArgumentsDetails AddFlagDetail(string flagName, string flagDescription, int flagCapacity = 0, List<string> autoComplete = null) {
            ArgumentsDetails argumentsDetail = new ArgumentsDetails(this);

            if(!FlagDescription.ContainsKey(flagName))
                argumentsDetail.FlagDescription.TryAdd(flagName, flagDescription);
            else
                argumentsDetail.FlagDescription[flagName] = flagDescription;

            if(!FlagParameterCapacity.ContainsKey(flagName))
                argumentsDetail.FlagParameterCapacity.TryAdd(flagName, flagCapacity);
            else
                argumentsDetail.FlagParameterCapacity[flagName] = flagCapacity;

            if(!FlagAutoComplete.ContainsKey(flagName))
                argumentsDetail.FlagAutoComplete.TryAdd(flagName, autoComplete != null ? autoComplete : new List<string>());
            else
                argumentsDetail.FlagAutoComplete[flagName].AddRange(autoComplete != null ? autoComplete : new List<string>());

            return argumentsDetail;
        }

        public void Foreach(Action<string, string, int> action) {
            foreach(KeyValuePair<string, string> flag in FlagDescription) {
                action(flag.Key, flag.Value, FlagParameterCapacity[flag.Key]);
            }
        }
    }

    public class Arguments {
        public Dictionary<string, List<string>> FlagParameterPairs { get; private set; } = new Dictionary<string, List<string>>();
        public ArgumentsDetails ArgumentsDetail { get; private set; }

        public string? GetParametersAtFlag(string flag, int index = 0) {
            if(!FlagParameterPairs.ContainsKey(flag)) return null;
            if(index >= FlagParameterPairs[flag].Count) return null;

            return FlagParameterPairs[flag][index];
        }

        public void AddNewFlag(string flag, int capacity = 0) {
            FlagParameterPairs.TryAdd(flag, new List<string>(capacity));
        }

        public void AddParameterToFlag(string flag, string parameter) {
            if(!FlagParameterPairs.ContainsKey(flag)) return;
            FlagParameterPairs[flag].Add(parameter);
        }

        public Arguments(ArgumentsDetails argumentsDetail) {
            ArgumentsDetail = argumentsDetail;

            argumentsDetail.Foreach((flagName, flagDescription, flagCapacity) => {
                AddNewFlag(flagName, flagCapacity);
            });
        }

        public string? this[string flag, int index] {
            get {
                return GetParametersAtFlag(flag, index);
            }
        }
    }

    public class Command {
        public string CommandName { get; internal set; }
        public string CommandDescription { get; internal set; }
        public ArgumentsDetails CommandArgsDetail { get; internal set; }
        public ICommand CommandImplementation { get; internal set; }
    }

    public interface ICommand {
        void OnExecute(Arguments args, string whereBeingExecuted, bool executeDirectly);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute {
        public string CommandName { get; private set; }
        public string CommandDescription { get; private set;  }

        public CommandAttribute(string commandName, string commandDescription = "None") {
            CommandName = commandName;
            CommandDescription = commandDescription;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ArgumentsDetailAttribute : Attribute {
        public string FlagName { get; private set; }
        public string FlagDescription { get; private set; }
        public int FlagParameterCapacity { get; private set; }
        public List<string> AutoComplete { get; set; }

        public ArgumentsDetailAttribute(string flagName, string flagdDescription, int flagParameterCapacity, List<string> autoComplete = null) {
            FlagName = flagName;
            FlagDescription = flagdDescription;
            FlagParameterCapacity = flagParameterCapacity;
            AutoComplete = autoComplete;
        }

        public ArgumentsDetailAttribute(string flagName, string flagdDescription, int flagParameterCapacity) {
            FlagName = flagName;
            FlagDescription = flagdDescription;
            FlagParameterCapacity = flagParameterCapacity;
            AutoComplete = null;
        }
    }
}
