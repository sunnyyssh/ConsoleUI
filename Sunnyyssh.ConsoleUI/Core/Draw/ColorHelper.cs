namespace Sunnyyssh.ConsoleUI;

internal static class ColorHelper
{
    internal static ConsoleColor ToConsoleColor(Color color) => (ConsoleColor)(color - 2);
}