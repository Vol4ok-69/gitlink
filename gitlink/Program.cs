using IWshRuntimeLibrary;

namespace gitlink;

class Program
{

    static void Main(string[] args)
    {
        if (args.Length == 0 || !args[0].Equals("create", StringComparison.CurrentCultureIgnoreCase))
        {
            Console.WriteLine("gitlink v1.1 made by Vol4ok69");
            Console.WriteLine("Try: gitlink create");
            return;
        }

        foreach (string arg in args)
        {
            arg.ToLower().Trim();
        }
        try
        {
            string logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "GitSupStrLog.txt"
            );

            string targetDir = Environment.CurrentDirectory; // всегда текущая папка

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
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GitSupStrLog.txt"),
                $"Error: {e.Message}\n{e.StackTrace}"
            );
        }
    }

    public static void Log(string logPath, string message) =>
        System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {message + Environment.NewLine}");
}