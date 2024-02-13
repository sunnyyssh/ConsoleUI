using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

[DebuggerDisplay("{DebuggerDisplay}")]
public sealed class PixelInfo
{
    private string DebuggerDisplay => !IsVisible ? "not visible" : $"c= '{Char}'; f= {Foreground}; b= {Background}";

    public static PixelInfo VisibleEmpty => new(' ', Color.Transparent, Color.Transparent);
    
    public bool IsVisible { get; private set; }
    
    public Color Foreground { get; private init; }
    
    public Color Background { get; private init; }
    
    public char Char { get; private init; }

    public PixelInfo(char c, Color background = Color.Default, Color foreground = Color.Default)
    {
        if (IsCharSpecial(c))
            throw new ArgumentException(@"The char must not be one of { \n \r \t \b \f \v \a }.", nameof(c));
        Char = c;
        Foreground = foreground;
        Background = background;
        IsVisible = true;
    }

    /// <summary>
    /// Creates a pixel filled by one speicific color.
    /// </summary>
    /// <param name="color">Color to fill pixel with.</param>
    public PixelInfo(Color color) : this(' ', color, Color.Transparent)
    { }

    /// <summary>
    /// Creates a non-visible pixel.
    /// </summary>
    public PixelInfo() : this('\u0000', Color.Transparent, Color.Transparent)
    {
        IsVisible = false;
    }

    private static readonly char[] ProhibitedChars = { '\n', '\t', '\r', '\b', '\f', '\v', '\a', };
    private static bool IsCharSpecial(char c)
    {
        for (int i = 0; i < ProhibitedChars.Length; i++)
            if (ProhibitedChars[i] == c)
                return true;
        return false;
    }
}