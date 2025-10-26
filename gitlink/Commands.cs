namespace gitlink
{
    public sealed class Commands
    {
        public string Command { get; }

        private Commands(string flag) => Command = flag;

        public static readonly Commands Create = new("create");

        /// <summary>
        /// Write version gitlink
        /// </summary>
        public static readonly Commands Version = new("version");

        /// <summary>
        /// Help command
        /// </summary>
        public static readonly Commands Help = new("help");

        /// <summary>
        /// No command
        /// </summary>
        public static readonly Commands None = new("");

        public static readonly List<Commands> AllCommands =
        [
            None,
            Create,
            Version,
            Help
        ];

        /// <returns>Flag in string representation</returns>
        public override string ToString() => Command;

        public static Commands GetCommand(string commandStr)
        {
            if (commandStr == null) return None;
            foreach (var command in AllCommands)
            {
                if (command != null && command.ToString().Equals(commandStr, System.StringComparison.Ordinal))
                    return command;
            }
            return None;
        }
    }
}