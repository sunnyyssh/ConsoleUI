using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

[DebuggerDisplay("{DebuggerDisplay}")]
public sealed class PixelInfo
{
    private string DebuggerDisplay => !IsVisible ? "not visible" : $"c= '{Char}'; f= {Foreground}; b= {Background}";
    public bool IsVisible { get; private set; }
    public Color Foreground { get; private init; }
    public Color Background { get; private init; }
    public char Char { get; private init; }

    public PixelInfo(char c, Color foreground, Color background)
    {
        Char = c;
        Foreground = foreground;
        Background = background;
        IsVisible = true;
    }

    public PixelInfo() : this('\u0000', Color.Transparent, Color.Transparent)
    {
        IsVisible = false;
    }
}