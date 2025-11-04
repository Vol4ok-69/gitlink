using System.Diagnostics;

namespace gitlink
{
    public static class CommandRunner
    {
        public static string RunCommand(string fileName, string arguments, string workingDirectory)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,

                //настройки для перенаправления ввода/вывода
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = startInfo };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    return $"Error (Exit Code {process.ExitCode}) in directory '{workingDirectory}': {error}";
                }

                return output;
            }
            catch (Exception ex)
            {
                return $"Error running '{fileName} {arguments}': {ex.Message}";
            }
        }
    }
}