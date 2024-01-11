namespace Sunnyyssh.ConsoleUI;

public struct PixelInfo
{
    public Color Foreground { get; private init; }
    public Color Background { get; private init; }
    public char Char { get; private init; }

    public PixelInfo(char c, Color foreground, Color background)
    {
        Char = c;
        Foreground = foreground;
        Background = background;
    }
}