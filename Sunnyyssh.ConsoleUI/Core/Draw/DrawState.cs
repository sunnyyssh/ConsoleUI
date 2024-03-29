using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Represents the draw state.
/// </summary>
public sealed class DrawState
{
    /// <summary>
    /// The empty draw state.
    /// </summary>
    public static DrawState Empty => new DrawState(PixelLineCollection.Empty);
    
    /// <summary>
    /// Lines of the draw state. The state actually consists of them.
    /// </summary>
    public PixelLineCollection Lines { get; }

    /// <summary>
    /// Creates the instance of <see cref="DrawState"/> with given lines.
    /// </summary>
    /// <param name="lines">Lines that draw state'll consist of.</param>
    public DrawState(PixelLineCollection lines)
    {
        Lines = lines ?? throw new ArgumentNullException(nameof(lines));
    }

    /// <summary>
    /// Tries to get pixel
    /// </summary>
    /// <param name="left">Left coordinate.</param>
    /// <param name="top">Top coordinate.</param>
    /// <param name="resultPixel">Result pixel if it's found. null otherwise.</param>
    /// <returns>True if pixel was found. False otherwise.</returns>
    public bool TryGetPixel(int left, int top, [NotNullWhen(true)] out PixelInfo? resultPixel)
    {
        foreach (PixelLine line in Lines)
        {
            if (line.Top != top)
                continue;
            if (left < line.Left || left >= line.Left + line.Left)
                continue;
            resultPixel = line[left - line.Left];
            return true;
        }

        resultPixel = null;
        return false;
    }

    [Pure]
    public DrawState OverlapWith(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        return Combine(this, state);
    }

    [Pure]
    public DrawState HideOverlapWith(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        return HideOverlap(this, state);
    }

    [Pure]
    public DrawState Crop(int left, int top, int width, int height)
    {
        var cropped = Lines 
            // Removing lines not matching vertical bounds 
            .Where(line => line.Top >= top && line.Top < top + height)
            // Removing lines not matching horizntal bounds.
            .Where(line => line.Left + line.Length > left && line.Left < left + width)
            .Select(line => line.Crop(left - line.Left, left + width - line.Left))
            .ToCollection();
        return new DrawState(cropped);
    }
    
    [Pure]
    public DrawState Shift(int leftShift, int topShift)
    {
        return new DrawState(
            Lines
            .Select(l => 
                new PixelLine(
                    l.Left + leftShift, 
                    l.Top + topShift, 
                    l.Pixels))
            .ToCollection());
    }

    [Pure]
    public DrawState SubtractWith(DrawState deductible)
    {
        ArgumentNullException.ThrowIfNull(deductible, nameof(deductible));

        var newLines = Lines.ToArray();
        
        for (int i = 0; i < newLines.Length; i++)
        {
            foreach (var deductibleLine in deductible.Lines)
            {
                if (deductibleLine.Top != newLines[i].Top)
                    continue;
                newLines[i] = newLines[i].Subtract(deductibleLine);
            }
        }

        return new DrawState(newLines.ToCollection());
    }
    
    public static DrawState Combine(params DrawState[] orderedDrawStates)
    {
        ArgumentNullException.ThrowIfNull(orderedDrawStates, nameof(orderedDrawStates));

        var lines = orderedDrawStates
            // making one sequence of lines from sequence of sequences of lines.
            .SelectMany(
                state => state.Lines,
                (_, line) => line)
            // Grouping lines by their Top value
            // and making one line from each group using overlapping
            .GroupBy(
                line => line.Top,
                line => line,
                (_, lines) => PixelLine.Overlap(lines.ToArray()))
            .ToCollection();

        DrawState result = new(lines);
        return result;
    }

    public static DrawState HideOverlap(params DrawState[] orderedDrawStates)
    {
        ArgumentNullException.ThrowIfNull(orderedDrawStates, nameof(orderedDrawStates));
        
        var lines = orderedDrawStates
            // making one sequence of lines from sequence of sequences of lines.
            .SelectMany(
                state => state.Lines,
                (_, line) => line)
            // Grouping lines by their Top value
            // and making one line from each group using hide overlapping
            .GroupBy(
                line => line.Top,
                line => line,
                (_, lines) => PixelLine.HideOverlap(lines.ToArray()))
            .ToCollection();

        DrawState result = new(lines);
        return result;
    }
}
