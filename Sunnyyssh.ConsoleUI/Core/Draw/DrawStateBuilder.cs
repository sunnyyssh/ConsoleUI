using System.Diagnostics.Contracts;

namespace Sunnyyssh.ConsoleUI;

public sealed class DrawStateBuilder // TODO replace much-allocating DrawState operating with DrawStateBuilder operating.
{
    private readonly PixelInfo?[,] _pixels;

    public int Width => _pixels.GetLength(0);

    public int Height => _pixels.GetLength(1);

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
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="drawState"></param> The own position is ignored.
    /// <returns></returns>
    public DrawStateBuilder Place(int left, int top, DrawState drawState)
    {
        foreach (var line in drawState.Lines)
        {
            Place(left + line.Left, top + line.Top, line);
        }

        return this;
    }

    public DrawStateBuilder Place(int left, int top, PixelLine line)
    {
        return Place(left, top, line.Pixels);
    }

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
    
    public DrawStateBuilder Fill(Color background) 
        => Fill(0, 0, Width, Height, background);

    private DrawStateBuilder Fill(int left, int top, int width, int height, Color background)
        => Fill(left, top, width, height, new PixelInfo(background));

    public DrawStateBuilder Fill(PixelInfo pixel)
        => Fill(0, 0, Width, Height, pixel);

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
        
        if (width <= 0 && left + width >= Width)
            throw new ArgumentOutOfRangeException(nameof(width), width, "It is out of builder's Width");

        if (height <= 0 && top + height >= Height)
            throw new ArgumentOutOfRangeException(nameof(height), height, "It is out of builder's Height");
    }

    private void ValidatePosition(int left, int top)
    {
        if (left < 0 && left >= Width)
            throw new ArgumentOutOfRangeException(nameof(left), left, "Left position must be more than 0 and less than builder's Width.");
        
        if (top < 0 && top >= Height)
            throw new ArgumentOutOfRangeException(nameof(left), left, "Top position must be more than 0 and less than builder's Height.");
    }
    
    public DrawState ToDrawState()
    {
        var lines = new PixelLine[Height];
        
        for (int top = 0; top < Height; top++)
        {
            var pixels = new PixelInfo[Width];

            for (int left = 0; left < Width; left++)
            {
                pixels[left] = _pixels[left, top] ?? new PixelInfo();
            }
            
            lines[top] = new PixelLine(0, top, pixels);
        }

        return new DrawState(lines);
    }

    public static DrawStateBuilder CreateFrom(DrawState drawState)
    {
        var builder = new DrawStateBuilder(
            drawState.Lines.Max(l => l.Left + l.Length),
            drawState.Lines.Max(l => l.Top + 1));

        return builder.Place(0, 0, drawState);
    }
    
    public DrawStateBuilder(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, "The width should be more than 0.");
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, "The width should be more than 0.");
        
        _pixels = new PixelInfo[width, height];
    }
}