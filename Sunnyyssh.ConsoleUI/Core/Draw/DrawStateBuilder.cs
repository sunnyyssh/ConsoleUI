using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Represents <see cref="DrawState"/> creating.
/// </summary>
/// <example>
/// <code>
/// var builder = new DrawStateBuilder(width, height);
///         
/// builder.Fill(Color.DarkGreen)
///     .Place(0, 0, @"Follow t.me/vowtostrive", Color.Green, Color.Black)
///     .Place(0, 1, $"{DateTime.Now}", Color.Green, Color.White)
///     .Fill(0, height - 1, width, 1, new PixelInfo());
///
/// 
/// DrawState result = builder.ToDrawState();
/// </code>
/// </example>
public sealed class DrawStateBuilder 
{
    private readonly PixelInfo?[,] _pixels;

    /// <summary>
    /// The width of the state area.
    /// </summary>
    public int Width => _pixels.GetLength(0);

    /// <summary>
    /// The height of the state area.
    /// </summary>
    public int Height => _pixels.GetLength(1);

    /// <summary>
    /// Gets or sets <see cref="PixelInfo"/> pixel at specified position.
    /// </summary>
    /// <param name="left">Left position of the pixel. (Counted in characters).</param>
    /// <param name="top">Top position of the pixel. (Counted in characters).</param>
    public PixelInfo this[int left, int top]
    {
        get
        {
            ValidatePosition(left, top);
            
            return _pixels[left, top] ??= new PixelInfo();
        }
        set
        {
            ValidatePosition(left, top);
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _pixels[left, top] = value;
        }
    }

    #region Place methods.

    /// <summary>
    /// Places <see cref="string"/> to the specified position.
    /// </summary>
    /// <param name="left">Left position of placing string. (Counted in characters).</param>
    /// <param name="top">Top position of placing string. (Counted in characters).</param>
    /// <param name="line"><see cref="string"/> to place.</param>
    /// <param name="background">The background color of placed string.</param>
    /// <param name="foreground">The foreground color of placed string.</param>
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Place(int left, int top, string line, Color background = Color.Default, Color foreground = Color.Default)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));
        
        ValidateSquare(left, top, line.Length, 1);

        for (int i = 0; i < line.Length; i++)
        {
            _pixels[left + i, top] = new PixelInfo(line[i], background, foreground);
        }

        return this;
    }

    /// <summary>
    /// Places state at specified position (The own position of the state is ignored).
    /// </summary>
    /// <param name="left">Left position of placing state. (Counted in characters).</param>
    /// <param name="top">Top position of placing state. (Counted in characters).</param>
    /// <param name="drawState">State to place. Be accurate: The own position of the state is ignored.</param> 
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Place(int left, int top, DrawState drawState)
    {
        foreach (var line in drawState.Lines)
        {
            Place(left + line.Left, top + line.Top, line);
        }

        return this;
    }

    /// <summary>
    /// Places <see cref="PixelLine"/> line at specified position (The own position of the line is ignored).
    /// </summary>
    /// <param name="left">Left position of placing line. (Counted in characters).</param>
    /// <param name="top">Top position of placing line. (Counted in characters).</param>
    /// <param name="line">Line to place. Be accurate: The own position of the line is ignored.</param> 
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Place(int left, int top, PixelLine line)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));
        
        ValidateSquare(left, top, line.Pixels.Count, 1);

        for (int i = 0; i < line.Pixels.Count; i++)
        {
            _pixels[left + i, top] = line.Pixels[i];
        }

        return this;
    }

    /// <summary>
    /// Places <see cref="PixelInfo"/> pixels at specified position.
    /// </summary>
    /// <param name="left">Left position of placing line of pixels. (Counted in characters).</param>
    /// <param name="top">Top position of placing line of pixels. (Counted in characters).</param>
    /// <param name="pixels">Pixels to place.</param> 
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Place(int left, int top, PixelInfo[] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));
        
        ValidateSquare(left, top, pixels.Length, 1);

        for (int i = 0; i < pixels.Length; i++)
        {
            _pixels[left + i, top] = pixels[i];
        }

        return this;
    }

    /// <summary>
    /// Places <see cref="PixelInfo"/> pixels at specified position.
    /// </summary>
    /// <param name="left">Left position of placing area of pixels. (Counted in characters).</param>
    /// <param name="top">Top position of placing area of pixels. (Counted in characters).</param>
    /// <param name="pixels">Pixels to place.</param> 
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Place(int left, int top, PixelInfo[,] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

        int width = pixels.GetLength(1);
        int height = pixels.GetLength(0);
        
        ValidateSquare(left, top, width, height);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                _pixels[left + j, top + i] = pixels[i, j];
            }
        }

        return this;
    }

    #endregion

    #region Fill methods.
    
    /// <summary>
    /// Fills the whole area with given color. (Color used as background).
    /// </summary>
    /// <param name="background">Color to fill with.</param>
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Fill(Color background) 
        => Fill(0, 0, Width, Height, background);

    /// <summary>
    /// Fills specified area with given color. (Color used as background).
    /// </summary>
    /// <param name="left">Left position of area to fill. (Counted in characters).</param>
    /// <param name="top">Top position of area to fill. (Counted in characters).</param>
    /// <param name="width">Width of area to fill. (Counted in characters).</param>
    /// <param name="height">Height of area to fill. (Counted in characters).</param>
    /// <param name="background">Color to fill with.</param>
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    private DrawStateBuilder Fill(int left, int top, int width, int height, Color background)
        => Fill(left, top, width, height, new PixelInfo(background));

    /// <summary>
    /// Fills the whole area with given <see cref="PixelInfo"/> pixel.
    /// </summary>
    /// <param name="pixel">Pixel to fill with.</param>
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Fill(PixelInfo pixel)
        => Fill(0, 0, Width, Height, pixel);

    /// <summary>
    /// Fills specified area with given <see cref="PixelInfo"/>; pixel.
    /// </summary>
    /// <param name="left">Left position of area to fill. (Counted in characters).</param>
    /// <param name="top">Top position of area to fill. (Counted in characters).</param>
    /// <param name="width">Width of area to fill. (Counted in characters).</param>
    /// <param name="height">Height of area to fill. (Counted in characters).</param>
    /// <param name="pixel">Pixel to fill with.</param>
    /// <returns>Same instance of <see cref="DrawStateBuilder"/> to chain calls.</returns>
    public DrawStateBuilder Fill(int left, int top, int width, int height, PixelInfo pixel)
    {
        ArgumentNullException.ThrowIfNull(pixel, nameof(pixel));

        ValidateSquare(left, top, width, height);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                // PixelInfo is read-only, so we can set all references to one instance.
                _pixels[left + j, top + i] = pixel;
            }
        }

        return this;
    }

    #endregion
    
    private void ValidateSquare(int left, int top, int width, int height)
    {
        ValidatePosition(left, top);
        
        if (width <= 0 && left + width > Width)
            throw new ArgumentOutOfRangeException(nameof(width), width, "It is out of builder's Width");

        if (height <= 0 && top + height > Height)
            throw new ArgumentOutOfRangeException(nameof(height), height, "It is out of builder's Height");
    }

    private void ValidatePosition(int left, int top)
    {
        if (left < 0 && left >= Width)
            throw new ArgumentOutOfRangeException(nameof(left), left, "Left position must be more than 0 and less than builder's Width.");
        
        if (top < 0 && top >= Height)
            throw new ArgumentOutOfRangeException(nameof(left), left, "Top position must be more than 0 and less than builder's Height.");
    }
    
    /// <summary>
    /// Creates <see cref="DrawState"/> instance with pixels that are set earlier. Not set pixels are not visible.
    /// </summary>
    /// <returns>Result state.</returns>
    public DrawState ToDrawState()
    {
        var lines = new PixelLine[Height];
        
        for (int top = 0; top < Height; top++)
        {
            int thisTop = top;
            
            var pixels = Enumerable.Range(0, Width)
                .Select(left => _pixels[left, thisTop] ?? new PixelInfo())
                .ToImmutableList();
            
            lines[top] = new PixelLine(0, top, pixels);
        }

        return new DrawState(lines.ToImmutableList());
    }

    /// <summary>
    /// Creates <see cref="DrawStateBuilder"/> instance from given <see cref="DrawState"/> instance with its size and its pixels.
    /// Be careful: modifying builder doesn't modify initial state.
    /// </summary>
    /// <param name="drawState">State to initialize from.</param>
    /// <returns>Created builder.</returns>
    public static DrawStateBuilder CreateFrom(DrawState drawState)
    {
        ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
        
        var builder = new DrawStateBuilder(
            drawState.Lines.Max(l => l.Left + l.Length),
            drawState.Lines.Max(l => l.Top + 1));

        return builder.Place(0, 0, drawState);
    }
    
    /// <summary>
    /// Creates an instance of <see cref="DrawStateBuilder"/>
    /// </summary>
    /// <param name="width">The width of building area.</param>
    /// <param name="height">The height of building area.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DrawStateBuilder(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "The width should be more than 0.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "The width should be more than 0.");
        
        _pixels = new PixelInfo[width, height];
    }
}