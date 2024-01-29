// Tested type.

using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

[DebuggerDisplay("{DebuggerDisplay}")]
public sealed class PixelLine
{
    private string DebuggerDisplay =>
        $"({Left}; {Top}) - {Length} : \"{string.Concat(Pixels.Select(p => p.IsVisible ? p.Char : ' '))}\"";
    
    public PixelInfo[] Pixels { get; private init; }

    public int Length => Pixels.Length;
    
    public int Left { get; private init; }
    
    public int Top { get; private init; }

    public PixelInfo this[int n] => Pixels[n];

    public PixelLine(int left, int top, Color background, Color foreground, string line)
    {
        ArgumentNullException.ThrowIfNull(line, nameof(line));
        var pixels = Enumerable.Range(0, line.Length)
            .Select(i => new PixelInfo(line[i], background, foreground))
            .ToArray();
        Left = left;
        Top = top;
        Pixels = pixels;
    }

    public PixelLine(int left, int top, PixelInfo[] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));
        Left = left;
        Top = top;
        Pixels = pixels;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="orderedLines">Lines array in order of higher overlapping. The forther line is in the array, the higher it is.</param>
    /// <returns></returns>
    public static PixelLine Overlap(params PixelLine[] orderedLines)
    {
        ArgumentNullException.ThrowIfNull(orderedLines, nameof(orderedLines));
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
                    pixels[offset] = new PixelInfo(line[i].Char, line[i].Foreground,
                        pixels[offset]!.Background);
                    continue;
                }
                
                pixels[offset] = line[i];
            }
        }

        // null pixels must be PixelInfo with IsVisible = false.
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] ??= new PixelInfo();

        return new PixelLine(leftInclusive, top, pixels!);
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
}