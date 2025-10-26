namespace gitlink
{
    public sealed class Flags
    {
        public string Flag { get; }

        private Flags(string flag) => Flag = flag;

        /// <summary>
        /// Creates a git repository, adds all files to Index,
        /// commits those files with the message "Initial commit",
        /// creates a shortcut to git bash,
        /// creates .gitignore and adds all .* files in directory to it.
        /// </summary>
        public static readonly Flags All = new("-a");

        /// <summary>
        /// Creates a git repository, adds all files to Index,
        /// commits those files with the message "Initial commit",
        /// </summary>
        public static readonly Flags Git = new("-g");

        public static readonly Flags Shortcut = new("-s");

        /// <summary>
        /// Creates .gitignore and adds all .* files in directory to it.
        /// </summary>
        public static readonly Flags GitIgnore = new("-gi");

        /// <summary>
        /// Without flag
        /// </summary>
        public static readonly Flags None = new("");

        public static readonly List<Flags> AllFlags = new List<Flags>
        {
            None,
            All,
            Git,
            GitIgnore,
            Shortcut
        };

        /// <returns>Flag in string representation</returns>
        public override string ToString() => Flag;

        public static Flags GetFlag(string flagStr)
        {
            if (flagStr == null) return None;
            foreach (var flag in AllFlags)
                if (flag != null && flag.ToString().Equals(flagStr, System.StringComparison.Ordinal))
                    return flag;
            return None;
        }
    }
}
