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
                    Print("Error: Arguments should not be repeated.", ConsoleColor.Red);
                    return;
                }

                var command = Command.GetCommand(_args[0]);
                if (command == Command.None)
                {
                    Print("Error: The first argument must be command. [help] to view all commands.", ConsoleColor.Red);
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

                    case "status":
                        Log(_logPath, $"dir: {_targetDir}");
                        CommandStatus();
                        break;

                    case "help":
                        Log(_logPath, $"dir: {_targetDir}");
                        CommandHelp();
                        break;

                    default:
                        Print("Please enter a command. Use [help] to see available commands.", ConsoleColor.Yellow);
                        return;
                }
            }
            else
            {
                Print("Error: no arguments provided. Use [help] to see available commands.", ConsoleColor.Red);
                return;
            }
            Console.WriteLine();
        }

        #region Команды
        public static void CommandCreate()
        {
            try
            {
                if (_selectedFlags.Contains(Flag.All) && _selectedFlags.Count > 1)
                {
                    Print("Cannot specify other flags when flag [-a] is specified", ConsoleColor.Red);
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
                    Print(msg, ConsoleColor.Red);
                    Log(_logPath, msg);
                    return;
                }

                if (doShortcut)
                {
                    string shortcutPath = Path.Combine(_targetDir, "Git Bash.lnk");
                    if (System.IO.File.Exists(shortcutPath))
                    {
                        string msg = $"Shortcut already exist.";
                        Print(msg, ConsoleColor.Yellow);
                        Log(_logPath, msg);
                    }
                    else
                    {
                        string gitBashPath = @"C:\Program Files\Git\git-bash.exe";
                        if (!System.IO.File.Exists(gitBashPath))
                        {
                            gitBashPath = @"C:\Program Files (x86)\Git\git-bash.exe";
                            if (!System.IO.File.Exists(gitBashPath))
                                Log(_logPath, "Git Bash not found in Program Files.");
                        }


                        try
                        {
                            var wsh = new WshShell();
                            var sc = (IWshShortcut)wsh.CreateShortcut(shortcutPath);
                            sc.TargetPath = System.IO.File.Exists(gitBashPath) ? gitBashPath : "git";
                            sc.WorkingDirectory = _targetDir;
                            sc.Description = "Git Bash in this repository";
                            sc.Save();

                            string msg = $"Shortcut created: '{shortcutPath}'.";
                            Print(msg, ConsoleColor.Green);
                            Log(_logPath, msg);
                        }
                        catch (Exception ex)
                        {
                            string msg = $"Failed to create shortcut: {ex.Message}";
                            Print(msg, ConsoleColor.Red);
                            Log(_logPath, msg);
                        }
                    }
                }

                if (doGitIgnore)
                {
                    string repoRoot = _targetDir;
                    while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
                    {
                        string? parent = Directory.GetParent(repoRoot)?.FullName;
                        if (parent == null || parent == repoRoot)
                            break;
                        repoRoot = parent;
                    }

                    if (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
                    {
                        Print("Warning: .git folder not found in parent directories. Using current directory.", ConsoleColor.Yellow);
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
                            .Select(l => l.Trim().TrimEnd('/'))
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToHashSet(StringComparer.OrdinalIgnoreCase)
                        : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (!isExist)
                    {
                        System.IO.File.WriteAllText(gitIgnorePath, string.Empty);
                        isExist = true;
                        Log(_logPath, $".gitignore created: '{gitIgnorePath}'.");
                        Print($".gitignore created: '{gitIgnorePath}'", ConsoleColor.Green);
                    }

                    List<string> foundCandidates = [];
                    foreach (var path in candidatePaths)
                    {
                        string nameToCheck = path.TrimEnd('/');
                        string fullPath = Path.Combine(_targetDir, nameToCheck);

                        //добавляем в кандидаты только если существует в проекте
                        if (Directory.Exists(fullPath) || System.IO.File.Exists(fullPath))
                            if (!existingLines.Contains(nameToCheck))  //не добавлено в .gitignore
                                foundCandidates.Add(path);
                    }

                    if (foundCandidates.Count == 0)
                        Print("No new candidates to add to .gitignore (everything already added).", ConsoleColor.Gray);
                    else
                    {
                        Print("Found the following candidates to add to .gitignore:", ConsoleColor.Cyan);
                        foreach (var p in foundCandidates) Print($"  {p}", ConsoleColor.Gray);
                        Print("Options: [y] add this, [n] skip, [a] add this and all remaining", ConsoleColor.Gray);

                        for (int i = 0; i < foundCandidates.Count; i++)
                        {
                            string path = foundCandidates[i];
                            string nameToCheck = path.TrimEnd('/');

                            while (true)
                            {
                                Print($"Add '{path}' to .gitignore? [y/n/a] ", ConsoleColor.Cyan, newline: false);
                                var input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

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
                                            Print(msg, ConsoleColor.Green);
                                            Log(_logPath, msg);
                                            existingLines.Add(remName);
                                        }
                                    }
                                    i = foundCandidates.Count;
                                    break;
                                }

                                if (input == "y" || input == "yes")
                                {
                                    System.IO.File.AppendAllText(gitIgnorePath, path + Environment.NewLine);
                                    string msgAdded = $"Added to .gitignore: '{path}'";
                                    Print(msgAdded, ConsoleColor.Green);
                                    Log(_logPath, msgAdded);
                                    existingLines.Add(nameToCheck);
                                    break;
                                }

                                if (input == "n" || input == "no" || input == "")
                                    break;

                                Print("Please answer 'y', 'n' or 'a'.", ConsoleColor.Yellow);
                            }
                        }
                    }
                }


                if (doGit)
                {
                    if (!Directory.Exists(Path.Combine(_targetDir, ".git")))
                    {
                        string initOut = CommandRunner.RunCommand(gitExecutable!, "init", _targetDir);
                        Print(initOut.Trim(), ConsoleColor.Green);
                        Log(_logPath, $"git init output: {initOut}");
                    }
                    else
                        Log(_logPath, $"Repository already initialized in '{_targetDir}'.");

                    string renameOut = CommandRunner.RunCommand(gitExecutable!, "branch -M main", _targetDir);
                    Log(_logPath, $"git rename output: {renameOut}");

                    string addOut = CommandRunner.RunCommand(gitExecutable!, "add .", _targetDir);
                    Log(_logPath, $"git add output: {addOut}");

                    string commitOut = CommandRunner.RunCommand(gitExecutable!, "commit -m \"Initial commit\"", _targetDir);
                    if (commitOut.StartsWith("Error"))
                    {
                        Log(_logPath, $"git commit failed or nothing to commit: {commitOut}");
                        Print("Warning: commit may have failed (check git config or no changes). See log.", ConsoleColor.Yellow);
                    }
                    else
                        Log(_logPath, $"git commit output: {commitOut}");
                }
            }
            catch (Exception e)
            {
                Print($"Error: {e.Message}", ConsoleColor.Red);
                Log(_logPath, $"Error: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void CommandStatus()
        {
            try
            {
                string repoRoot = _targetDir;
                while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
                {
                    string? parent = Directory.GetParent(repoRoot)?.FullName;
                    if (parent == null || parent == repoRoot)
                        break;
                    repoRoot = parent;
                }

                bool repoExists = Directory.Exists(Path.Combine(repoRoot, ".git"));
                bool shortcutExists = System.IO.File.Exists(Path.Combine(_targetDir, "Git Bash.lnk"));
                string gitIgnorePath = Path.Combine(repoRoot, ".gitignore");
                bool gitignoreExists = System.IO.File.Exists(gitIgnorePath);

                //репозиторий
                if (repoExists)
                    Print("Git repository detected (.git found)", ConsoleColor.Green);
                else
                    Print("No Git repository found", ConsoleColor.Red);

                //Ярлык
                if (shortcutExists)
                    Print("Shortcut 'Git Bash.lnk' exists", ConsoleColor.Green);
                else
                    Print("Shortcut 'Git Bash.lnk' not found", ConsoleColor.Yellow);

                //.gitignore
                if (gitignoreExists)
                {
                    string[] lines = System.IO.File.ReadAllLines(gitIgnorePath);
                    var existing = lines.Select(l => l.Trim().TrimEnd('/'))
                                        .Where(l => !string.IsNullOrWhiteSpace(l))
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

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

                    //оставляем только реально существующие элементы
                    List<string> existingInFs = [];
                    foreach (var p in candidatePaths)
                    {
                        string checkPath = Path.Combine(_targetDir, p.TrimEnd('/'));
                        if (Directory.Exists(checkPath) || System.IO.File.Exists(checkPath))
                            existingInFs.Add(p);
                    }

                    if (existingInFs.Count == 0)
                    {
                        Print("No matching files or directories found to check in .gitignore.", ConsoleColor.Gray);
                    }
                    else
                    {
                        var missing = existingInFs
                            .Where(p => !existing.Contains(p.TrimEnd('/')))
                            .ToList();

                        if (missing.Count == 0)
                            Print("All existing items are already in .gitignore", ConsoleColor.Green);
                        else
                        {
                            Print("Some existing items are missing in .gitignore:", ConsoleColor.Yellow);
                            foreach (var m in missing)
                                Print($"   {m}", ConsoleColor.Gray);
                        }
                    }
                }
                else
                {
                    Print(".gitignore not found", ConsoleColor.Red);
                }

                Log(_logPath, $"Status checked for '{_targetDir}'");
            }
            catch (Exception ex)
            {
                string msg = $"Error during status check: {ex.Message}";
                Print(msg, ConsoleColor.Red);
                Log(_logPath, msg);
            }
        }

        public static void CommandVersion() =>
            Print("gitlink v1.3 made by Vol4ok69", ConsoleColor.Cyan);

        public static void CommandHelp()
        {
            Print("All commands:", ConsoleColor.Cyan);
            Print($"[{string.Join(", ", _allNotNoneCommands)}]\n", ConsoleColor.Gray);
            Print("All flags:", ConsoleColor.Cyan);
            Print($"[{string.Join(", ", _allNotNoneFlags)}]\n", ConsoleColor.Gray);
            Print("Usage example:", ConsoleColor.Cyan);
            Print("  gitlink create -a      # create repo, .gitignore and shortcut", ConsoleColor.Gray);
            Print("  gitlink create -g      # only git init/add/commit", ConsoleColor.Gray);
            Print("  gitlink create -gi     # only .gitignore additions", ConsoleColor.Gray);
            Print("  gitlink create -s      # only create shortcut", ConsoleColor.Gray);
        }

        #endregion

        private static void Print(string text, ConsoleColor color = ConsoleColor.Gray, bool newline = true)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (newline) Console.WriteLine(text);
            else Console.Write(text);
            Console.ForegroundColor = prev;
        }

        public static void InitializeArgs(string[]? args)
        {
            if (_isInitialized)
                return;
            _args = args;
            //_args =
            //[
            //    "create",
            //    "-s",
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
