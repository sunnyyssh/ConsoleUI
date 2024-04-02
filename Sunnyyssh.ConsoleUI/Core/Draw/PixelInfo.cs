using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Represents the character and its background and foreground colors. (Or indicates its non-visibility).
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public sealed class PixelInfo
{
    // It's for displaying by debugger.
    private string DebuggerDisplay => !IsVisible ? "not visible" : $"c= '{Char}'; f= {Foreground}; b= {Background}";
    
    /// <summary>
    /// Indicates if pixel is visible.
    /// </summary>
    public bool IsVisible { get; }
    
    /// <summary>
    /// The foreground color of the pixel.
    /// </summary>
    public Color Foreground { get; }
    
    /// <summary>
    /// The background color of the pixel.
    /// </summary>
    public Color Background { get; }
    
    /// <summary>
    /// The character of the pixel.
    /// </summary>
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

    /// <summary>
    /// Creates an instance of <see cref="PixelInfo"/> with specified character and colors.
    /// </summary>
    /// <param name="c">The character of the pixel.</param>
    /// <param name="background">The background color.</param>
    /// <param name="foreground">The foreground color.</param>
    /// <exception cref="ArgumentException">Char was special</exception>
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