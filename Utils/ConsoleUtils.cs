using Utils.Guards;

namespace Utils;

public static class ConsoleUtils
{
    public static void ClearLastConsoleLines(int numberLines)
    {
        Guard.IsGreater(numberLines, 0);
        var linesToClearCount = Console.CursorTop - numberLines;
        Guard.IsGreater(linesToClearCount, 0);
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, linesToClearCount);
    }
}
