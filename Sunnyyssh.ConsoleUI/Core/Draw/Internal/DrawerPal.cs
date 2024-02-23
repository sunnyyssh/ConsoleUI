using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sunnyyssh.ConsoleUI;
    
// The type is expected to be inherited
// because it's possible that different platforms should have different console drawing.
// This type is not thread-safe. But it's used only in thread-safe context.
internal class DrawerPal
{ 
    /// <summary>
    /// Stores the previous (latest) state that is drawn in console.
    /// </summary>
    protected DrawState PreviousState = DrawState.Empty;
    
    // Indicates if it's expected to throw an exception on trying to draw outside the buffer.
    private readonly bool _borderConflictsAllowed;

    // The default background color
    protected readonly Color DefaultBackground;

    // The default foreground color
    protected readonly Color DefaultForeground;
    
    public int BufferWidth => Console.WindowWidth;

    public int BufferHeight => Console.WindowHeight;

    public void Clear() => Console.Clear();
    
    // Handles start of drawing. 
    // It may be implemented by platfrom-specific DrawerPal inheritors.
    public virtual void OnStart() { }
    
    // /// <summary>
    // /// Redraws the latest state of console.
    // /// </summary>
    //public void Redraw(CancellationToken cancellationToken) => DrawSingleRequest(PreviousState, cancellationToken);
    
    /// <summary>
    /// Draws the request directly in console.
    /// </summary>
    /// <param name="drawState">The state to draw.</param>
    /// <param name="cancellationToken"></param>
    public virtual void DrawSingleRequest(DrawState drawState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
        
        // Checking if state is outside of buffer size.
        ValidateBorderConflicts(drawState);
        // If exception on border violation is not thrown 
        // I should crop the DrawState instance to the actual buffer size.
        drawState = drawState.Crop(0, 0, BufferWidth, BufferHeight);

        foreach (PixelLine line in drawState.Lines)
        {
            DrawLine(line, cancellationToken);
        }
        // Reseting cursor position to (0, 0) point.
        Console.SetCursorPosition(0, 0);
        // Storing actual state as latest.
        RenewPreviousState(drawState);
    }

    // Draws the single line.
    protected virtual void DrawLine(PixelLine line, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        // Trying to optimize drawing by minimizing Console.Write method invocations.
        var parts = LineToParts(line);
        
        foreach (PartLine partLine in parts)
        {
            DrawPart(partLine, cancellationToken);
        }
    }

    // Splits single line to same-colored parts. 
    protected PartLine[] LineToParts(PixelLine line)
    {
        // Optimizing the drawing is mainly in minimizing the count of Console methods invocations.
        // So the less parts line's splitted, the faster drawing is.
        List<PartLine> resultParts = new();
        
        for (int startIndex = 0; startIndex < line.Length;)
        {
            // If part can be extracted it's added to the result collection.
            if (TryExtractPart(line, ref startIndex, out var newPart))
            {
                resultParts.Add(newPart);
            }
        }
        
        return resultParts.ToArray();
    }
    
    // Tries to extract part starting at given index. 
    private bool TryExtractPart(PixelLine sourceLine, ref int startIndex, [NotNullWhen((true))] out PartLine? result)
    {
        int endIndex = startIndex;
        ConsoleColor? lastBackground = null;
        ConsoleColor? lastForeground = null;
        List<string> subParts = new();
        while (ExtractNotVisible(sourceLine, ref lastBackground, ref lastForeground, 
                   ref startIndex, out var notVisibleRow))
        {
            if (notVisibleRow is not null)
            {
                subParts.Add(notVisibleRow);
            }
            subParts.Add(ExtractVisible(sourceLine, ref lastBackground, ref lastForeground, ref startIndex));
            endIndex = startIndex;
        }

        if (!subParts.Any())
        {
            result = null;
            return false;
        }
        
        result = new PartLine(
            sourceLine.Left + endIndex - subParts.Sum(p => p.Length),
            sourceLine.Top,
            lastBackground ?? ToConsoleBackgroundColor(Color.Transparent),
            lastForeground ?? ToConsoleForegroundColor(Color.Transparent),
            string.Concat(subParts));
        return true;
    }

    private string ExtractVisible(PixelLine sourceLine, ref ConsoleColor? lastBackground, 
        ref ConsoleColor? lastForeground, ref int startIndex)
    {
        StringBuilder row = new();

        for (; startIndex < sourceLine.Length; startIndex++)
        {
            PixelInfo current = sourceLine[startIndex];
            if (!current.IsVisible)
                break;
            
            if (!lastBackground.HasValue)
            {
                lastBackground = ToConsoleBackgroundColor(current.Background);
            }
            else
            {
                if (lastBackground != ToConsoleBackgroundColor(current.Background))
                    break;
            }
            
            if (current.Foreground == Color.Transparent)
            {
                row.Append(' ');
                continue;
            }
            
            if (!lastForeground.HasValue)
            {
                lastForeground = ToConsoleForegroundColor(current.Foreground);
            }
            else
            {
                if (lastForeground != ToConsoleForegroundColor(current.Foreground))
                    break;
            }
            
            row.Append(current.Char);
        }

        return row.ToString();
    }

    private bool ExtractNotVisible(PixelLine sourceLine, ref ConsoleColor? lastBackground, 
        ref ConsoleColor? lastForeground, ref int startIndex,
        out string? result)
    {
        StringBuilder row = new();
        bool allSameColored = !lastForeground.HasValue || !lastBackground.HasValue;
        
        for (; startIndex < sourceLine.Length; startIndex++)
        {
            if (sourceLine[startIndex].IsVisible)
                break;
            if (!allSameColored)
                continue;
            
            if (!PreviousState.TryGetPixel(sourceLine.Left + startIndex, sourceLine.Top, out var underlying))
                underlying = new PixelInfo();

            if (!underlying.IsVisible)
            {
                if (ToConsoleBackgroundColor(Color.Transparent) != lastBackground)
                    allSameColored = false;
                else
                    row.Append(' ');
                continue;
            }

            if (underlying.Foreground == Color.Transparent)
            {
                if (ToConsoleBackgroundColor(underlying.Background) != lastBackground)
                    allSameColored = false;
                else
                    row.Append(' ');
                continue;
            }

            if (ToConsoleForegroundColor(underlying.Foreground) != lastForeground)
            {
                allSameColored = false;
                continue;
            }
            
            if (ToConsoleBackgroundColor(underlying.Background) != lastBackground)
            {
                allSameColored = false;
                continue;
            }

            row.Append(underlying.Char);
        }

        if (!allSameColored || startIndex == sourceLine.Length)
        {
            result = null;
            return false;
        }

        if (row.Length == 0)
        {
            result = null;
            if (lastBackground.HasValue || lastForeground.HasValue)
                return false;
            lastBackground = lastForeground = null; 
        }
        else
        {
            result = row.ToString();
        }
        
        return true;
    }
    
    protected virtual void DrawPart(PartLine part, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        Console.SetCursorPosition(part.Left, part.Top);
        Console.BackgroundColor = part.Background;
        Console.ForegroundColor = part.Foreground;
        
        Console.Write(part.Part);
    }

    protected ConsoleColor ToConsoleBackgroundColor(Color color)
    {
        if (color == Color.Transparent || color == Color.Default)
            return ToConsoleColor(DefaultBackground);
        return (ConsoleColor)(color - 2);
    }
    
    protected ConsoleColor ToConsoleForegroundColor(Color color)
    {
        if (color == Color.Transparent)
            return ToConsoleColor(DefaultBackground);
        if (color == Color.Default)
            return ToConsoleColor(DefaultForeground);
        return ToConsoleColor(color);
    }

    private static ConsoleColor ToConsoleColor(Color color) => (ConsoleColor)(color - 2);

    protected void RenewPreviousState(DrawState newState)
    {
        PreviousState = DrawState.Combine(PreviousState, newState);
    }

    protected void ValidateBorderConflicts(DrawState drawState)
    {
        if (_borderConflictsAllowed)
            return;
        
        foreach (PixelLine line in drawState.Lines)
        {
            if (line.Top >= BufferHeight)
            {
                throw new DrawingException("Buffer height is less than it's needed. ");
            }

            if (line.Left + line.Length >= BufferWidth)
            {
                throw new DrawingException("Buffer width is less than it's needed. ");
            }
        }
    }
    
    public DrawerPal(Color defaultBackground, Color defaultForeground,  bool borderConflictsAllowed)
    {
        DefaultBackground = defaultBackground;
        DefaultForeground = defaultForeground;
        _borderConflictsAllowed = borderConflictsAllowed;
    }

    protected record PartLine(int Left, int Top, ConsoleColor Background, ConsoleColor Foreground, string Part);

}