namespace gitlink
{
    public sealed class Command
    {
        private string CommandStr { get; }

        private Command(string flag) => CommandStr = flag;

        public static readonly Command Create = new("create");

        /// <summary>
        /// Write status
        /// </summary>
        public static readonly Command Status = new("status");

        /// <summary>
        /// Write gitlink version
        /// </summary>
        public static readonly Command Version = new("version");

        /// <summary>
        /// Help command
        /// </summary>
        public static readonly Command Help = new("help");

        /// <summary>
        /// No command
        /// </summary>
        public static readonly Command None = new("");

        public static readonly List<Command> AllCommands =
        [
            None,
            Create,
            Version,
            Status,
            Help
        ];

        /// <returns>Flag in string representation</returns>
        public override string ToString() => CommandStr;

        public static Command GetCommand(string commandStr)
        {
            if (commandStr == null) return None;
            foreach (var command in AllCommands)
                if (command != null && command.ToString().Equals(commandStr, System.StringComparison.Ordinal))
                    return command;
            return None;
        }
    }
}