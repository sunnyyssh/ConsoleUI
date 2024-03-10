using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

[DebuggerDisplay("{DebuggerDisplay}")]
public sealed class PixelInfo
{
    private string DebuggerDisplay => !IsVisible ? "not visible" : $"c= '{Char}'; f= {Foreground}; b= {Background}";

    public static PixelInfo VisibleEmpty => new(' ', Color.Transparent, Color.Transparent);
    
    public bool IsVisible { get; }
    
    public Color Foreground { get; }
    
    public Color Background { get; }
    
    public char Char { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not PixelInfo pixel)
            return false;
        return Equals(pixel);
    }

    private bool Equals(PixelInfo pixel)
    {
        return Char == pixel.Char
               && Foreground == pixel.Foreground
               && Background == pixel.Background
               && IsVisible == pixel.IsVisible;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsVisible, (int)Foreground, (int)Background, Char);
    }

    public PixelInfo(char c, Color background = Color.Default, Color foreground = Color.Default)
    {
        if (CharHelper.IsCharSpecial(c))
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

    public static bool operator ==(PixelInfo left, PixelInfo right) => left.Equals(right);

    public static bool operator !=(PixelInfo left, PixelInfo right)
    {
        return !(left == right);
    }
}