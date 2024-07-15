// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Represents a line of <see cref="PixelInfo"/> pixels at specified position.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public sealed class PixelLine
{
    // It's for displaying by debugger.
    private string DebuggerDisplay =>
        $"({Left}; {Top}) - {Length} : \"{string.Concat(Pixels.Select(p => p.IsVisible ? p.Char : ' '))}\"";
    
    /// <summary>
    /// Pixels collection which this line consits of.
    /// </summary>
    public ImmutableList<PixelInfo> Pixels { get; }

    /// <summary>
    /// The length of the line.
    /// </summary>
    public int Length => Pixels.Count;
    
    /// <summary>
    /// Left position of the line. (Counted in characters).
    /// </summary>
    public int Left { get; }
    
    /// <summary>
    /// Top position of the line. (Counted in characters).
    /// </summary>
    public int Top { get; }

    /// <summary>
    /// Gets pixel at specified position in line.
    /// </summary>
    /// <param name="n">The position in line.</param>
    public PixelInfo this[int n] => Pixels[n];

    /// <summary>
    /// Crops line with specified range.
    /// </summary>
    /// <param name="startIndex">Start index (in pixels collection) of the range. It also can be negative.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>Cropped line.</returns>
    [Pure]
    public PixelLine Crop(int startIndex, int length)
    {
        var cropped = Pixels.Skip(startIndex).Take(length).ToImmutableList();
        
        return new PixelLine(Left + Math.Max(startIndex, 0), Top, cropped);
    }

    /// <summary>
    /// Subtracts with line with <see cref="deductible"/>.
    /// </summary>
    /// <param name="deductible">Deductible line.</param>
    /// <returns>The result of subtraction.</returns>
    [Pure]
    public PixelLine Subtract(PixelLine deductible)
    {
        ArgumentNullException.ThrowIfNull(deductible, nameof(deductible));
        
        if (Top != deductible.Top || !IsIntersectedWith(deductible))
            return this;

        var newPixels = Pixels.ToArray();
        
        int leftSubtractionBound = Math.Max(0, deductible.Left - Left);
        int deductibleOffset = Left - deductible.Left;
        int rightSubtractionBound = Math.Min(Length, deductible.Left + deductible.Length - Left);
        
        for (int i = leftSubtractionBound; i < rightSubtractionBound; i++)
        {
            var currentPixel = newPixels[i];
            var deductiblePixel = deductible[i + deductibleOffset];
            if (!deductiblePixel.IsVisible)
            {
                continue;
            }

            if (deductiblePixel.Background != Color.Transparent)
            {
                newPixels[i] = new PixelInfo();
                continue;
            }

            newPixels[i] = new PixelInfo(
                deductiblePixel.Char, 
                currentPixel.Background, 
                deductiblePixel.Foreground);
        }

        return new PixelLine(Left, Top, newPixels.ToImmutableList());
    }

    /// <summary>
    /// Indicates if this line is intersected with <see cref="line"/>.
    /// </summary>
    /// <param name="line">Line to check</param>
    /// <returns>True if intersetced. False otherwise.</returns>
    public bool IsIntersectedWith(PixelLine line)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));

        return line.Left < Left + Length && Left < line.Left + line.Length;
    }
    
    /// <summary>
    /// Creates an instance of <see cref="PixelLine"/> with given position, string and colors.
    /// </summary>
    /// <param name="left">The left position of the line. (Counted in characters).</param>
    /// <param name="top">The top position of the line. (Counted in characters).</param>
    /// <param name="background">Background color.</param>
    /// <param name="foreground">Foreground color.</param>
    /// <param name="line">Set of characters of line.</param>
    public PixelLine(int left, int top, Color background, Color foreground, string line)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));
        
        var pixels = Enumerable.Range(0, line.Length)
            .Select(i => new PixelInfo(line[i], background, foreground))
            .ToImmutableList();
        
        Left = left;
        Top = top;
        Pixels = pixels;
    }

    /// <summary>
    /// Creates an instance of <see cref="PixelLine"/> with given position and pixels.
    /// </summary>
    /// <param name="left">The left position of the line. (Counted in characters).</param>;
    /// <param name="top">The top position of the line. (Counted in characters).</param>;
    /// <param name="pixels">Initial pixels.</param>
    public PixelLine(int left, int top, ImmutableList<PixelInfo> pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));
        
        Left = left;
        Top = top;
        Pixels = pixels;
    }

    /// <summary>
    /// Overlaps collection of lines of one top position. If line is earlier in collection then it is overlapped by later ones.
    /// It respects not visible pixels and transparent colors. So, for example, if overlapping pixel is not visible underlying one is resolved.
    /// </summary>
    /// <param name="orderedLines">Lines array in order of higher overlapping. The farther line is in the array, the higher it is.</param>
    /// <returns>Result line.</returns>
    public static PixelLine Overlap([Pure] params PixelLine[] orderedLines)
    {
        ArgumentNullException.ThrowIfNull(orderedLines, nameof(orderedLines));
        
        if (orderedLines.Length == 0)
            return new PixelLine(0, 0, ImmutableList<PixelInfo>.Empty);
        
        CheckLinesTopEquality(orderedLines);

        int top = orderedLines.First().Top;
        int leftInclusive = orderedLines.Select(l => l.Left).Min();
        int rightExclusive = orderedLines.Select(l => l.Left + l.Length).Max();
        int length = rightExclusive - leftInclusive;

        PixelInfo?[] pixels = new PixelInfo[length];

        foreach (PixelLine line in orderedLines)
        {
            for (int i = 0, offset = line.Left - leftInclusive; i < line.Length; i++, offset++)
            {
                // If pixel isn't visible than we shouldn't change anything.
                if (!line[i].IsVisible)
                    continue;

                if (pixels[offset] is null)
                {
                    pixels[offset] = line[i];
                    continue;
                }
                
                if (line[i].Background == Color.Transparent)
                {
                    // If background is transparent then background should be taken from the lower line.
                    pixels[offset] = new PixelInfo(
                        line[i].Char, 
                        pixels[offset]!.Background,
                        line[i].Foreground);
                    continue;
                }
                
                pixels[offset] = line[i];
            }
        }

        // null pixels must be PixelInfo with IsVisible = false.
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] ??= new PixelInfo();

        return new PixelLine(leftInclusive, top, pixels.ToImmutableList()!);
    }
    
    /// <summary>
    /// Naively combines ordered collection of <see cref="PixelLine"/> instances. If line is earlier in collection then it is naively overlapped (or hidden) by later ones.
    /// It means that overlapping pixel always hides underlying. It doesn't respect non-visibility and transparency.
    /// </summary>
    /// <param name="orderedLines">Lines array in order of higher overlapping. The farther line is in the array, the higher it is.</param>
    /// <returns>Result line.</returns>
    public static PixelLine HideOverlap([Pure] PixelLine[] orderedLines)
    {
        ArgumentNullException.ThrowIfNull(orderedLines, nameof(orderedLines));
        
        if (orderedLines.Length == 0)
            return new PixelLine(0, 0, ImmutableList<PixelInfo>.Empty);
        
        CheckLinesTopEquality(orderedLines);

        int top = orderedLines.First().Top;
        int leftInclusive = orderedLines.Select(l => l.Left).Min();
        int rightExclusive = orderedLines.Select(l => l.Left + l.Length).Max();
        int length = rightExclusive - leftInclusive;

        PixelInfo?[] pixels = new PixelInfo[length];

        foreach (PixelLine line in orderedLines)
        {
            for (int i = 0, offset = line.Left - leftInclusive; i < line.Length; i++, offset++)
            {
                pixels[offset] = line[i];
            }
        }

        // null pixels must be PixelInfo with IsVisible = false.
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] ??= new PixelInfo();

        return new PixelLine(leftInclusive, top, pixels.ToImmutableList()!);
    }
    
    private static void CheckLinesTopEquality(PixelLine[] lines)
    {
        if (lines.Length == 0)
            throw new ArgumentException("Array must contain at least one element");
        
        if (lines.Any(l => l.Top != lines[0].Top))
        {
            throw new ArgumentException("Lines must have equal Top property.");
        }
    }

    /// <summary>
    /// Indicates if any lines are intersected.
    /// </summary>
    /// <param name="pixelLines">Lines to check.</param>
    /// <returns>True if eny lines are intersected. False otherwise.</returns>
    public static bool AreIntersected(params PixelLine[] pixelLines)
    {
        for (int i = 0; i < pixelLines.Length; i++)
        {
            var first = pixelLines[i];
            
            for (int j = i + 1; j < pixelLines.Length; j++)
            {
                var second = pixelLines[j];
                
                if (first.Top != second.Top)
                    continue;
                if (first.Left > second.Left + second.Length)
                    continue;
                if (second.Left < first.Left + first.Length)
                    continue;
                
                return false;
            }
        }

        return true;
    }
}