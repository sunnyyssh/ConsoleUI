namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Represents UI colors.
/// </summary>
public enum Color
{
    /// <summary>
    /// Default color. It's sepcified by <see cref="Application"/> default color.
    /// It can differ if it is background or foreground color.
    /// </summary>
    Default = 0,
    /// <summary>
    /// Transparent color. It means that underlying color will be set to this pixel.
    /// </summary>
    Transparent = 1,
    Black = (ConsoleColor.Black + 2),
    DarkBlue = (ConsoleColor.DarkBlue + 2),
    DarkGreen = (ConsoleColor.DarkGreen + 2),
    DarkCyan = (ConsoleColor.DarkCyan + 2),
    DarkRed = (ConsoleColor.DarkRed + 2),
    DarkMagenta = (ConsoleColor.DarkMagenta + 2),
    DarkYellow = (ConsoleColor.DarkYellow + 2),
    Gray = (ConsoleColor.Gray + 2),
    DarkGray = (ConsoleColor.DarkGray + 2),
    Blue = (ConsoleColor.Blue + 2),
    Green = (ConsoleColor.Green + 2),
    Cyan = (ConsoleColor.Cyan + 2),
    Red = (ConsoleColor.Red + 2),
    Magenta = (ConsoleColor.Magenta + 2),
    Yellow = (ConsoleColor.Yellow + 2),
    White = (ConsoleColor.White + 2),
}