namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Helps work with <see cref="Color"/>
/// </summary>
internal static class ColorHelper
{
    /// <summary>
    /// Coverts <see cref="Color"/> to <see cref="ConsoleColor"/>.
    /// </summary>
    /// <param name="color">Color to convert.</param>
    /// <returns>Converted color.</returns>
    internal static ConsoleColor ToConsoleColor(Color color) => (ConsoleColor)(color - 2);
}