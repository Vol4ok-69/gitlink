//using System.ComponentModel.DataAnnotations;
namespace gitlink;

public sealed class Flags
{
    public string Flag { get; }

    private Flags(string flag) => Flag = flag;

    public static readonly List<Flags> AllFlags =
    [
        All,
        Help,
        Git,
        GitIgnore,
        None
    ];

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

    /// <summary> 
    /// Write all commands and flags
    /// </summary>
    public static readonly Flags Help = new("--help");

    /// <summary> 
    /// Creates .gitignore and adds all .* files in directory to it.
    /// </summary>
    public static readonly Flags GitIgnore = new("-gi");

    /// <summary> 
    /// Without flag
    /// </summary>
    public static readonly Flags None = new("");

    /// <returns>Flag in string representation</returns>
    public override string ToString() => Flag;
    public static Flags GetFlag(string flagStr)
    {
        foreach (var flag in AllFlags)
            if (flag.ToString().Equals(flagStr))
                return flag;
        return None;
    }
}
