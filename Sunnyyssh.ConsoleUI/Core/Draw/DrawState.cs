// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;
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
    public static DrawState Empty => new DrawState(ImmutableList<PixelLine>.Empty);
    
    /// <summary>
    /// Lines of the draw state. The state actually consists of them.
    /// </summary>
    public ImmutableList<PixelLine> Lines { get; }

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

    /// <summary>
    /// Overlaps this state with <see cref="state"/>. It respects not visible pixels and transparent colors. So, for example, if overlapping pixel is not visible underlying one is resolved.
    /// </summary>
    /// <param name="state">State to overlap over this one.</param>
    /// <returns>Result state.</returns>
    [Pure]
    public DrawState OverlapWith(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        return Combine(this, state);
    }

    
    /// <summary>
    /// Naively overlaps this state with <see cref="state"/>. It means that overlapping pixel always hides underlying.
    /// It doesn't respect non-visibility and transparency.
    /// </summary>
    /// <param name="state">State to hide-overlap with.</param>
    /// <returns>Result state.</returns>
    [Pure]
    public DrawState HideOverlapWith(DrawState state)
    {
        ArgumentNullException.ThrowIfNull(state, nameof(state));

        return HideOverlap(this, state);
    }

    /// <summary>
    /// Crops draw state to specified area.
    /// </summary>
    /// <param name="left">Left position of area to crop to.</param>
    /// <param name="top">Top position of area to crop to.</param>
    /// <param name="width">Width of area to crop to.</param>
    /// <param name="height">Height of area to crop to.</param>
    /// <returns>Cropped draw state.</returns>
    [Pure]
    public DrawState Crop(int left, int top, int width, int height)
    {
        var cropped = Lines 
            // Removing lines not matching vertical bounds 
            .Where(line => line.Top >= top && line.Top < top + height)
            // Removing lines not matching horizntal bounds.
            .Where(line => line.Left + line.Length > left && line.Left < left + width)
            .Select(line => line.Crop(left - line.Left, left + width - line.Left))
            .ToImmutableList();
        return new DrawState(cropped);
    }
    
    /// <summary>
    /// Shifts (or moves) state to the new area.
    /// </summary>
    /// <param name="leftShift">Horizontal shift. If it's positive state shifts to the right. If negative - to the left.</param>
    /// <param name="topShift">Vertical shift. If it's positive state shifts down. If negative - up.</param>
    /// <returns>Shifted state.</returns>
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
            .ToImmutableList());
    }

    /// <summary>
    /// Subtracts this state with <see cref="deductible"/>.
    /// </summary>
    /// <param name="deductible">The deductible state.</param>
    /// <returns>The result of subtraction.</returns>
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

        return new DrawState(newLines.ToImmutableList());
    }
    
    /// <summary>
    /// Combines ordered collection of <see cref="DrawState"/> instances. If state is earlier in collection then it is overlapped by later ones.
    /// It respects not visible pixels and transparent colors. So, for example, if overlapping pixel is not visible underlying one is resolved.
    /// </summary>
    /// <param name="orderedDrawStates">Collection of combining states.</param>
    /// <returns>Combined state.</returns>
    public static DrawState Combine([Pure] params DrawState[] orderedDrawStates)
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
            .ToImmutableList();

        DrawState result = new(lines);
        return result;
    }

    /// <summary>
    /// Naively combines ordered collection of <see cref="DrawState"/> instances. If state is earlier in collection then it is naively overlapped (or hidden) by later ones.
    /// It means that overlapping pixel always hides underlying. It doesn't respect non-visibility and transparency.
    /// </summary>
    /// <param name="orderedDrawStates">Collection of combining states.</param>
    /// <returns>Combined state.</returns>
    public static DrawState HideOverlap([Pure] params DrawState[] orderedDrawStates)
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
            .ToImmutableList();

        DrawState result = new(lines);
        return result;
    }

    /// <summary>
    /// Creates the instance of <see cref="DrawState"/> with given lines.
    /// </summary>
    /// <param name="lines">Lines that draw state'll consist of.</param>
    public DrawState(ImmutableList<PixelLine> lines)
    {
        Lines = lines ?? throw new ArgumentNullException(nameof(lines));
    }
}

