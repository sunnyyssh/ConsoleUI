﻿// This type is not thread-safe. But it's used only in thread-safe context.

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sunnyyssh.ConsoleUI;
    
// The type is not sealed because it's possible that different platforms should have different console drawing.
internal class DrawerPal
{
    #region Fields and Properties

    protected InternalDrawState PreviousState = InternalDrawState.Empty;
    
    private bool _throwOnBorderConflict;

    protected readonly Color DefaultBackground;

    protected readonly Color DefaultForeground;

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
        List<PartLine> resultParts = new();
        
        for (int startIndex = 0; startIndex < line.Length;)
        {
            if (TryExtractPart(line, ref startIndex, out var newPart))
            {
                resultParts.Add(newPart);
            }
        }
        
        return resultParts.ToArray();
    }
    
    private bool TryExtractPart(PixelLine sourceLine, ref int startIndex, [NotNullWhen((true))] out PartLine? result)
    {
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
        }

        if (!subParts.Any())
        {
            result = null;
            return false;
        }
        
        result = new PartLine(
            sourceLine.Left + startIndex - subParts.Sum(p => p.Length),
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
    
    public DrawerPal(Color defaultBackground, Color defaultForeground,  bool throwOnBorderConflict)
    {
        DefaultBackground = defaultBackground;
        DefaultForeground = defaultForeground;
        _throwOnBorderConflict = throwOnBorderConflict;
    }

    protected record PartLine(int Left, int Top, ConsoleColor Background, ConsoleColor Foreground, string Part);
}