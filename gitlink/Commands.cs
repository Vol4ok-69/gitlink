//using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace gitlink;

public sealed class Commands
{
    public string Command { get; }

    private Commands(string flag) => Command = flag;

    public static readonly List<Commands?> AllCommands =
    [
        Create
    ];
    /// <summary> 
    /// 
    /// </summary>
    public static readonly Commands Create = new("create");

    /// <summary> 
    /// No command
    /// </summary>
    public static readonly Commands None = new("");

    /// <returns>Flag in string representation</returns>
    public override string ToString() => Command;

    public static Commands GetCommand(string commandStr)
    {
        foreach (var command in AllCommands)
            if (command.ToString().Equals(commandStr))
                return command;
        return None;
    }
}
