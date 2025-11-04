namespace gitlink
{
    public sealed class Flag
    {
        private string FlagStr { get; }

        private Flag(string flag) => FlagStr = flag;

        /// <summary>
        /// Creates a git repository, adds all files to Index,
        /// commits those files with the message "Initial commit",
        /// creates a shortcut to git bash,
        /// creates .gitignore and adds all .* files in directory to it.
        /// Also creates .dockerignore and Dockerfile.
        /// </summary>
        public static readonly Flag All = new("-a");

        /// <summary>
        /// Creates a git repository, adds all files to Index,
        /// commits those files with the message "Initial commit",
        /// </summary>
        public static readonly Flag Git = new("-g");

        public static readonly Flag Shortcut = new("-s");

        /// <summary>
        /// Creates .gitignore and adds relevant files/directories to it.
        /// </summary>
        public static readonly Flag GitIgnore = new("-gi");

        /// <summary>
        /// Creates .dockerignore and adds relevant files/directories to it.
        /// </summary>
        public static readonly Flag DockerIgnore = new("-di");

        /// <summary>
        /// Creates a Dockerfile with basic configuration.
        /// </summary>
        public static readonly Flag DockerFile = new("-df");

        /// <summary>
        /// Without flag
        /// </summary>
        public static readonly Flag None = new("");

        public static readonly List<Flag> AllFlags =
        [
            None,
            All,
            Git,
            GitIgnore,
            Shortcut,
            DockerIgnore,
            DockerFile
        ];

        /// <returns>Flag in string representation</returns>
        public override string ToString() => FlagStr;

        public static Flag GetFlag(string flagStr)
        {
            if (flagStr == null) return None;
            foreach (var flag in AllFlags)
                if (flag != null && flag.ToString().Equals(flagStr, System.StringComparison.Ordinal))
                    return flag;
            return None;
        }
    }
}
