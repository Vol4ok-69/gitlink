using IWshRuntimeLibrary;

namespace gitlink;

//dotnet publish -r win-x64 -c Release --self-contained true

class Program
{
    private static string[]? _args;
    private static bool _isInitialized = false;
    //флаги
    private static readonly List<Flags> _allFlags = Flags.AllFlags;
    private static readonly List<Flags> _allNotNoneFlags = [.. Flags.AllFlags.Where(f => f != Flags.None)];
    private static List<Flags> _selectedFlags = [];
    //команды
    private static readonly List<Commands> _allCommands = Commands.AllCommands;
    private static readonly List<Commands> _allNotNoneCommands = [.. Commands.AllCommands.Where(c => c != Commands.None)];
    private static Commands _selectedCommand;

    static void Main(string[] args)
    {
        InitializeArgs(args);
        //Console.WriteLine(string.Join(',', args));
        if (_args is not null && _args.Length > 0 && _args == _args.Distinct())
        {
            FindFlags();
            FindCommand();
            switch (_selectedCommand.ToString())
            {
                case "create":
                    CommandCreate();
                    break;

                case "version":
                    CommandVersion();
                    break;

                case "help":
                    CommandHelp();
                    break;

                default:
                    Console.WriteLine($"Please enter a command: [{string.Join(", ", _allNotNoneCommands)}]");
                    return;
            }
        }
        else if (_args != _args.Distinct())
        {
            Console.WriteLine("Error: Arguments should not be repeated.");
            return;
        }
        else
        {
            Console.WriteLine("Error: line 24");
            return;
        }
    }
    public static void CommandCreate()
    {
        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GitSupStrLog.txt");
        try
        {
            List<string> args = [];
            int k = 0;
            for (int i = 0; i < _args.Length; i++)
            {
                if (_args[k] == _args[i])
                {
                    args.Add(_args[i]); continue;
                }
            }
            if (_args.Contains(Flags.All.ToString()) && _args.Length > 2)
            {
                Console.WriteLine("Cannot specify other flags when flag -a is specified");
                return;
            }


            string targetDir = Environment.CurrentDirectory; //всегда текущая папка

            if (!Directory.Exists(Path.Combine(targetDir, ".git")))
            {
                string msg = $"'{targetDir}' not a Git perository (.git not found).";
                Console.WriteLine(msg);
                Log(logPath, msg);
                return;
            }

            string gitBashPath = @"C:\Program Files\Git\git-bash.exe";
            if (!System.IO.File.Exists(gitBashPath))
            {
                gitBashPath = @"C:\Program Files (x86)\Git\git-bash.exe";
                if (!System.IO.File.Exists(gitBashPath))
                {
                    string msg = "Git Bash not found. Make sure Git is installed.";
                    Console.WriteLine(msg);
                    Log(logPath, msg);
                    return;
                }
            }

            string shortcutPath = Path.Combine(targetDir, "Git Bash.lnk");
            var wsh = new WshShell();
            if (wsh.CreateShortcut(shortcutPath) is IWshShortcut shortcut)
            {
                //ярлык
                shortcut.TargetPath = gitBashPath;
                shortcut.WorkingDirectory = targetDir;
                shortcut.Description = "Git Bash in this repository";
                shortcut.Save();

                string msg = $"shortcut created: {shortcutPath}.";
                Console.WriteLine(msg);
                Log(logPath, msg);

                //.gitignore
                string gitIgnorePath = Path.Combine(targetDir, ".gitignore");
                string[] paths =
                [
                    ".vs/",
                    ".vscode/",
                    ".metadata/",
                    "Git Bash.lnk",
                    ".github/"
                ];
                bool isExist = System.IO.File.Exists(gitIgnorePath);
                string[] lines = isExist
                    ? System.IO.File.ReadAllLines(gitIgnorePath)
                    : [];
                bool isShowed = false;
                foreach (string path in paths)
                {
                    string nameToCheck = path.TrimEnd('/');
                    string fullPath = Path.Combine(targetDir, nameToCheck);

                    bool exists = Directory.Exists(fullPath) || System.IO.File.Exists(fullPath);
                    bool alreadyIgnored = lines.Any(line =>
                        line.Trim().TrimEnd('/') == path.TrimEnd('/'));

                    if (exists && !alreadyIgnored)
                    {
                        if (!isExist && !isShowed)
                        {
                            string msgGitIgnore = $".gitignore created: \'{targetDir}\'.";
                            Console.WriteLine(msgGitIgnore);
                            Log(logPath, msgGitIgnore);
                            isShowed = true;
                        }
                        System.IO.File.AppendAllText(gitIgnorePath, path + Environment.NewLine);
                        string msgGitIgnoreAdd = $"Added to .gitignore: '{path}'";
                        Console.WriteLine(msgGitIgnoreAdd);
                        Log(logPath, msgGitIgnoreAdd);
                    }
                }
            }
            else
            {
                string msg = $"Failed to create shortcut.";
                Console.WriteLine(msg);
                Log(logPath, msg);
            }
        }
        catch (Exception e)
        {
            Log(
                logPath, $"Error: {e.Message}\n{e.StackTrace}"
            );
        }
    }
    public static void CommandVersion()
    {
        Console.WriteLine("gitlink v1.1 made by Vol4ok69");
    }
    public static void CommandHelp()
    {
        Console.WriteLine("All commands: ");
        Console.WriteLine($"[{string.Join(", ", _allNotNoneCommands)}]");
    }

    public static void InitializeArgs(string[]? args)
    {
        if (_isInitialized)
            return;
        _args = args;
        _isInitialized = true;
    }
    public static void FindCommand()
    {
        Commands command = Commands.None;
        foreach (var arg in _args)
        {
            if (Commands.GetCommand(arg) != Commands.None)
                command = Commands.GetCommand(arg);
        }
        _selectedCommand = command;
    }
    public static void FindFlags()
    {
        foreach (string arg in _args)
        {
            if (Flags.GetFlag(arg) != Flags.None)
                _selectedFlags.Add(Flags.GetFlag(arg));
        }
    }
    public static void Log(string logPath, string message) =>
        System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {message + Environment.NewLine}");
}