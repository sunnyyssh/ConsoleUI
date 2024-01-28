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
        if (IsCharSpecial(c))
            throw new ArgumentException(@"The char must not be one of { \n \r \t \b \f \v \a }.", nameof(c));
        Char = c;
        Foreground = foreground;
        Background = background;
        IsVisible = true;
    }

    public PixelInfo() : this('\u0000', Color.Transparent, Color.Transparent)
    {
        IsVisible = false;
    }

    private static readonly char[] _prohibitedChars = { '\n', '\t', '\r', '\b', '\f', '\v', '\a', };
    private static bool IsCharSpecial(char c)
    {
        for (int i = 0; i < _prohibitedChars.Length; i++)
            if (_prohibitedChars[i] == c)
                return true;
        return false;
    }
}