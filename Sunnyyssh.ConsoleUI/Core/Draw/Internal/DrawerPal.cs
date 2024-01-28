// This type is not thread-safe. But it's used only in thread-safe context.

using System.Text;

namespace Sunnyyssh.ConsoleUI;
    
internal class DrawerPal
{
    #region Fields and Properties

    protected InternalDrawState PreviousState = InternalDrawState.Empty;
    
    private bool _throwOnBorderConflict;

    protected readonly Color DefaultBackground;

    public int BufferWidth => Console.BufferWidth;

    public int BufferHeight => Console.BufferHeight;

    #endregion

    public virtual void Clear() => Console.Clear();

    public void Redraw(CancellationToken cancellationToken) => DrawSingleRequest(PreviousState, cancellationToken);
    
    public virtual void DrawSingleRequest(InternalDrawState drawState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
        ValidateBorderConflicts(drawState);

        foreach (PixelLine line in drawState.Lines)
        {
            DrawLine(line, cancellationToken);
        }
        
        RenewPreviousState(drawState);
    }

    protected virtual void DrawLine(PixelLine line, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        var parts = LineToParts(line);
        
        foreach (PartLine partLine in parts)
        {
            DrawPart(partLine, cancellationToken);
        }
    }

    protected PartLine[] LineToParts(PixelLine line)
    {
        // Optimizing the drawing is mainly in minimizing the count of Console methods invocations.
        // So the less parts line's splitted, the faster drawing is.
        throw new NotImplementedException();
    }

    protected virtual void DrawPart(PartLine part, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        Console.SetCursorPosition(part.Left, part.Top);
        Console.BackgroundColor = ToConsoleColor(part.Background);
        Console.ForegroundColor = ToConsoleColor(part.Foreground);
        
        Console.Write(part.Part);
    }

    protected ConsoleColor ToConsoleColor(Color color)
    {
        if (color == Color.Transparent)
            return (ConsoleColor)(int)DefaultBackground;
        return (ConsoleColor)(int)color;
    }

    protected void RenewPreviousState(InternalDrawState newState)
    {
        PreviousState = InternalDrawState.Combine(PreviousState, newState);
    }

    protected void ValidateBorderConflicts(InternalDrawState drawState)
    {
        if (!_throwOnBorderConflict)
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
    
    public DrawerPal(bool throwOnBorderConflict)
    {
        _throwOnBorderConflict = throwOnBorderConflict;
    }

    protected record PartLine(int Left, int Top, Color Background, Color Foreground, string Part);
}