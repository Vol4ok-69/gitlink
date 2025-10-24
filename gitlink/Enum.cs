using System.ComponentModel.DataAnnotations;

namespace gitlink;

public sealed class Flags
{
    // 1. Поля для хранения значения и имени
    public string Flag { get; }

    // 2. Частный конструктор
    private Flags(string flag) => Flag = flag;

    // 3. Статические члены (замена enum)
    public static readonly Flags All = new Flags("-a");
    public static readonly Flags Help = new Flags("--help");

    // 4. Переопределение ToString для удобства
    public override string ToString() => Flag;

    // 5. Метод для парсинга (опционально)
    public static Flags FromString(string flag)
    {
        // ... Логика для поиска в коллекции All, Help и т.д.
        if (flag == All.Flag) return All;
        // ...
        throw new ArgumentException($"Unknown flag: {flag}");
    }
}
