namespace Sunnyyssh.ConsoleUI;

public static class ColorHelper
{
    public static ConsoleColor ToConsoleColor(Color color) => (ConsoleColor)(color - 2);
}