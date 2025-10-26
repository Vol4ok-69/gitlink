using IWshRuntimeLibrary;

namespace gitlink
{
    //              dotnet publish -r win-x64 -c Release --self-contained true

    class Program
    {

        #region Поля и свойства
        private static string[]? _args;
        private static bool _isInitialized = false;
        private static readonly string _targetDir = Environment.CurrentDirectory;
        private static readonly string _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GitSupStrLog.txt");

        //флаги
        private static readonly List<Flag> _allNotNoneFlags = [.. Flag.AllFlags.Skip(1)];
        private static List<Flag> _selectedFlags = [];

        //команды
        private static readonly List<Command> _allNotNoneCommands = [.. Command.AllCommands.Skip(1)];
        private static Command _selectedCommand = Command.None;
        #endregion

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            InitializeArgs(args);

            if (_args is not null && _args.Length > 0)
            {
                //проверка на повтор аргументов
                if (_args.Distinct(StringComparer.Ordinal).Count() != _args.Length)
                {
                    Console.WriteLine("Error: Arguments should not be repeated.");
                    return;
                }

                var command = Command.GetCommand(_args[0]);
                if (command == Command.None)
                {
                    Console.WriteLine("Error: The first argument must be command. [help] to view all commands.");
                    return;
                }
                _selectedCommand = command;

                FindFlags();

                switch (_selectedCommand.ToString())
                {
                    case "create":
                        Log(_logPath, $"dir: {_targetDir}");
                        CommandCreate();
                        break;

                    case "version":
                        Log(_logPath, $"dir: {_targetDir}");
                        CommandVersion();
                        break;

                    case "help":
                        Log(_logPath, $"dir: {_targetDir}");
                        CommandHelp();
                        break;

                    default:
                        Console.WriteLine($"Please enter a command. Use [help] to see available commands.");
                        return;
                }
            }
            else
            {
                Console.WriteLine("Error: no arguments provided. Use [help] to see available commands.");
                return;
            }
            Console.WriteLine();
        }

        #region Команды
        public static void CommandCreate()
        {
            try
            {
                //проверка на конфликт флага -a c другими
                if (_selectedFlags.Contains(Flag.All) && _selectedFlags.Count > 1)
                {
                    Console.WriteLine("Cannot specify other flags when flag [-a] is specified");
                    return;
                }

                bool doAll = _selectedFlags.Contains(Flag.All);
                bool doGit = doAll || _selectedFlags.Contains(Flag.Git);
                bool doGitIgnore = doAll || _selectedFlags.Contains(Flag.GitIgnore);
                bool doShortcut = doAll || _selectedFlags.Contains(Flag.Shortcut);

                string? gitExecutable = null;
                string gitExe1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "bin", "git.exe");
                string gitExe2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", "bin", "git.exe");
                if (System.IO.File.Exists(gitExe1)) gitExecutable = gitExe1;
                else if (System.IO.File.Exists(gitExe2)) gitExecutable = gitExe2;
                else
                {
                    try
                    {
                        var res = CommandRunner.RunCommand("git", "--version", Directory.GetCurrentDirectory());
                        if (!res.StartsWith("Error"))
                            gitExecutable = "git";
                    }
                    catch
                    {
                        gitExecutable = null;
                    }
                }

                if (doGit && gitExecutable == null)
                {
                    string msg = "Git not found. Make sure Git is installed and available in PATH.";
                    Console.WriteLine(msg);
                    Log(_logPath, msg);
                    return;
                }


                if (doShortcut)
                {
                    string gitBashPath = @"C:\Program Files\Git\git-bash.exe";
                    if (!System.IO.File.Exists(gitBashPath))
                    {
                        gitBashPath = @"C:\Program Files (x86)\Git\git-bash.exe";
                        if (!System.IO.File.Exists(gitBashPath))
                            Log(_logPath, "Git Bash not found in Program Files.");
                    }

                    string shortcutPath = Path.Combine(_targetDir, "Git Bash.lnk");
                    try
                    {
                        var wsh = new WshShell();
                        var sc = (IWshShortcut)wsh.CreateShortcut(shortcutPath);
                        sc.TargetPath = System.IO.File.Exists(gitBashPath) ? gitBashPath : "git";
                        sc.WorkingDirectory = _targetDir;
                        sc.Description = "Git Bash in this repository";
                        sc.Save();

                        string msg = $"Shortcut created: '{shortcutPath}'.";
                        Console.WriteLine(msg);
                        Log(_logPath, msg);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"Failed to create shortcut: {ex.Message}";
                        Console.WriteLine(msg);
                        Log(_logPath, msg);
                    }
                }

                if (doGitIgnore)
                {
                    //корневая папка репозитория
                    string repoRoot = _targetDir;
                    while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
                    {
                        string? parent = Directory.GetParent(repoRoot)?.FullName;
                        if (parent == null || parent == repoRoot)
                            break; //достигли корня диска
                        repoRoot = parent;
                    }

                    if (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
                    {
                        Console.WriteLine("Warning: .git folder not found in parent directories. Using current directory.");
                        repoRoot = _targetDir;
                    }

                    string gitIgnorePath = Path.Combine(repoRoot, ".gitignore");

                    string projectName = Path.GetFileName(repoRoot.TrimEnd(Path.DirectorySeparatorChar));

                    string[] candidatePaths =
                    [
                        projectName + "/bin/",
                        projectName + "/obj/",
                        ".vs/",
                        ".vscode/",
                        ".metadata/",
                        ".github/",
                        "Git Bash.lnk"
                    ];

                    bool isExist = System.IO.File.Exists(gitIgnorePath);
                    var existingLines = isExist
                        ? System.IO.File.ReadAllLines(gitIgnorePath)
                        .Select(l => l.Trim().TrimEnd('/')).ToHashSet(StringComparer.OrdinalIgnoreCase)
                        : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    List<string> foundCandidates = [];
                    foreach (var path in candidatePaths)
                    {
                        string nameToCheck = path.TrimEnd('/');
                        string fullPath = Path.Combine(_targetDir, nameToCheck);
                        if (Directory.Exists(fullPath) || System.IO.File.Exists(fullPath))
                            foundCandidates.Add(path);
                    }

                    if (foundCandidates.Count == 0)
                        Console.WriteLine("No candidate files or directories (.vs, .vscode, .metadata, Git Bash.lnk, .github, bin, obj) found to add to .gitignore.");

                    else
                    {
                        Console.WriteLine("Found the following candidates to add to .gitignore:");
                        foreach (var p in foundCandidates) Console.WriteLine($"  {p}");
                        Console.WriteLine("Options: [y] add this, [n] skip, [a] add this and all remaining");

                        if (!isExist)
                        {
                            System.IO.File.WriteAllText(gitIgnorePath, string.Empty);
                            isExist = true;
                            Log(_logPath, $".gitignore created: '{gitIgnorePath}'.");
                        }

                        for (int i = 0; i < foundCandidates.Count; i++)
                        {
                            string path = foundCandidates[i];
                            string nameToCheck = path.TrimEnd('/');
                            if (existingLines.Contains(nameToCheck))
                                continue;

                            while (true)
                            {
                                Console.Write($"Add '{path}' to .gitignore? [y/n/a] ");
                                var input = Console.ReadLine()?.
                                    Trim().
                                    ToLowerInvariant()
                                    ?? "";

                                if (input == "a" || input == "all")
                                {
                                    for (int j = i; j < foundCandidates.Count; j++)
                                    {
                                        string rem = foundCandidates[j];
                                        string remName = rem.TrimEnd('/');
                                        if (!existingLines.Contains(remName))
                                        {
                                            System.IO.File.AppendAllText(gitIgnorePath, rem + Environment.NewLine);
                                            string msg = $"Added to .gitignore: '{rem}'";
                                            Console.WriteLine(msg);
                                            Log(_logPath, msg);
                                            existingLines.Add(remName);
                                        }
                                    }
                                    i = foundCandidates.Count; //выход из фор внешнего
                                    break;
                                }

                                if (input == "y" || input == "yes")
                                {
                                    System.IO.File.AppendAllText(gitIgnorePath, path + Environment.NewLine);
                                    string msgAdded = $"Added to .gitignore: '{path}'";
                                    Console.WriteLine(msgAdded);
                                    Log(_logPath, msgAdded);
                                    existingLines.Add(nameToCheck);
                                    break;
                                }

                                if (input == "n" || input == "no" || input == "")
                                    break;

                                Console.WriteLine("Please answer 'y', 'n' or 'a'.");
                            }
                        }
                    }
                }

                if (doGit)
                {
                    if (!Directory.Exists(Path.Combine(_targetDir, ".git")))
                    {
                        string initOut = CommandRunner.RunCommand(gitExecutable!, "init", _targetDir);
                        Console.WriteLine(initOut.Trim());
                        Log(_logPath, $"git init output: {initOut}");
                    }
                    else
                        Log(_logPath, $"Repository already initialized in '{_targetDir}'.");

                    string addOut = CommandRunner.RunCommand(gitExecutable!, "add .", _targetDir);
                    Log(_logPath, $"git add output: {addOut}");

                    string commitOut = CommandRunner.RunCommand(gitExecutable!, "commit -m \"Initial commit\"", _targetDir);
                    if (commitOut.StartsWith("Error"))
                    {
                        Log(_logPath, $"git commit failed or nothing to commit: {commitOut}");
                        Console.WriteLine("Warning: commit may have failed (check git config or no changes). See log.");
                    }
                    else
                        Log(_logPath, $"git commit output: {commitOut}");
                }
            }
            catch (Exception e)
            {
                Log(_logPath, $"Error: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void CommandVersion() =>
            Console.WriteLine("gitlink v1.2 made by Vol4ok69");

        public static void CommandHelp()
        {
            Console.WriteLine("All commands: ");
            Console.WriteLine($"[{string.Join(", ", _allNotNoneCommands)}]\n");
            Console.WriteLine("All flags: ");
            Console.WriteLine($"[{string.Join(", ", _allNotNoneFlags)}]\n");
            Console.WriteLine("Usage example:");
            Console.WriteLine("  gitlink create -a      # create repo, .gitignore and shortcut");
            Console.WriteLine("  gitlink create -g      # only git init/add/commit");
            Console.WriteLine("  gitlink create -gi     # only .gitignore additions");
            Console.WriteLine("  gitlink create -s      # only create shortcut");
        }

        #endregion

        public static void InitializeArgs(string[]? args)
        {
            if (_isInitialized)
                return;
            _args = args;
            //_args =
            //[
            //    "create",
            //    "-gi"
            //];
            _isInitialized = true;
        }

        public static void FindFlags()
        {
            if (_args is null)
                return;

            foreach (var arg in _args.Skip(1))
            {
                var flag = Flag.GetFlag(arg);
                if (flag != Flag.None && !_selectedFlags.Contains(flag))
                    _selectedFlags.Add(flag);
            }
        }

        public static void Log(string logPath, string message) =>
            System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {message + Environment.NewLine}");
    }
}