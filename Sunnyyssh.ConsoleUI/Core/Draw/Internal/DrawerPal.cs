using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sunnyyssh.ConsoleUI;
    
// The type is expected to be inherited
// because it's possible that different platforms should have different console drawing.
// This type is not thread-safe. But it's used only in thread-safe context.
internal class DrawerPal
{
    
    // Indicates if it's expected to throw an exception on trying to draw outside the buffer.
    private readonly bool _borderConflictsAllowed;

    // The default background color
    protected readonly Color DefaultBackground;

    // The default foreground color
    protected readonly Color DefaultForeground;
    
    /// <summary>
    /// Stores the previous (latest) state that is drawn in console.
    /// </summary>
    protected PartLine[] PreviousState = Array.Empty<PartLine>();

    private ConsoleColor? _lastBackground;
    
    private ConsoleColor? _lastForeground;
    
    private int? _currentCursorLeft;
    
    private int? _currentCursorTop;
    
    public int BufferWidth => Console.WindowWidth;

    public int BufferHeight => Console.WindowHeight;

    public static int WindowWidth => Console.WindowWidth;

    public static int WindowHeight => Console.WindowHeight;

    public void Clear() => Console.Clear();
    
    /// <summary>
    /// Handles start of drawing. 
    /// It may be implemented by platfrom-specific DrawerPal inheritors.
    /// </summary>
    public virtual void OnStart()
    {
        Console.OutputEncoding = Encoding.Unicode;
    }
    
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

        var parts = drawState.Lines
            .SelectMany(LineToParts)
            .ToArray();

        foreach (var part in parts)
        {
            DrawPart(part, cancellationToken);
        }
        
        // Reseting cursor position to (0, 0) point.
        Console.SetCursorPosition(0, 0);
        // Storing actual state as latest.
        PreviousState = parts;
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
    private bool TryExtractPart(PixelLine sourceLine, ref int position, [NotNullWhen((true))] out PartLine? result)
    {
        int startPosition = position;

        ConsoleColor? background = null;
        ConsoleColor? foreground = null;
        
        StringBuilder resultBuilder = new();
        
        for ( ; position < sourceLine.Length; position++)
        {
            var current = sourceLine[position];
            if (current.IsVisible)
            {
                var currentBackground = ToConsoleBackgroundColor(current.Background);
                background ??= currentBackground;
                
                if (background != currentBackground)
                    break;

                if (current.Foreground == Color.Transparent)
                {
                    resultBuilder.Append(' ');
                    continue;
                }

                var currentForeground = ToConsoleForegroundColor(current.Foreground);
                foreground ??= currentForeground;
                
                if (foreground != currentForeground)
                    break;

                resultBuilder.Append(current.Char);
                continue;
            }
            
            var defaultBackground = ToConsoleBackgroundColor(Color.Default);
            background ??= defaultBackground;
            if (background != defaultBackground)
                break;

            resultBuilder.Append(' ');
        }

        background ??= ToConsoleBackgroundColor(Color.Default);
        foreground ??= background;

        result = new PartLine(
            sourceLine.Left + startPosition,
            sourceLine.Top,
            background.Value,
            foreground.Value,
            resultBuilder.ToString());
        
        return true;
    }

    protected virtual void DrawPart(PartLine part, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        SetCursorPosition(part.Left, part.Top, cancellationToken);
        
        SetBackgroundColor(part.Background, cancellationToken);
        
        SetForegroundColor(part.Foreground, cancellationToken);
        
        Write(part.Part, cancellationToken);
    }

    private void SetCursorPosition(int left, int top, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        if (left == _currentCursorLeft && top == _currentCursorTop)
            return;

        Console.SetCursorPosition(left, top);
        
        _currentCursorLeft = left;
        _currentCursorTop = top;
    }

    private void SetBackgroundColor(ConsoleColor color, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        if (_lastBackground == color)
            return;

        Console.BackgroundColor = color;

        _lastBackground = color;
    }

    private void SetForegroundColor(ConsoleColor color, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        if (_lastForeground == color)
            return;

        Console.ForegroundColor = color;

        _lastForeground = color;
    }

    private void Write(string line, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        Console.Write(line);
    }

    protected ConsoleColor ToConsoleBackgroundColor(Color color)
    {
        if (color == Color.Transparent || color == Color.Default)
            return ColorHelper.ToConsoleColor(DefaultBackground);
        return (ConsoleColor)(color - 2);
    }
    
    protected ConsoleColor ToConsoleForegroundColor(Color color)
    {
        if (color == Color.Transparent)
            return ColorHelper.ToConsoleColor(DefaultBackground);
        if (color == Color.Default)
            return ColorHelper.ToConsoleColor(DefaultForeground);
        return ColorHelper.ToConsoleColor(color);
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