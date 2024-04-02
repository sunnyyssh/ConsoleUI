using System.Collections;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Immutable collection of <see cref="PixelLine"/> instances.
/// </summary>
/// <example>
/// <code>
/// IEnumerable&lt;PixelLine&gt; lines = ...;
/// // First variant.
/// PixelLineCollection collection = lines.ToCollection();
/// // Second variant.
/// collection = PixelLineCollection.From(lines);
/// </code>
/// </example>
public sealed class PixelLineCollection : IReadOnlyList<PixelLine>
{
    public static readonly PixelLineCollection Empty = new(Array.Empty<PixelLine>());
    
    private readonly IReadOnlyList<PixelLine> _lines;

    public IEnumerator<PixelLine> GetEnumerator() => _lines.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _lines.GetEnumerator();

    public int Count => _lines.Count;

    public PixelLine this[int index] => _lines[index];

    public static PixelLineCollection From(IEnumerable<PixelLine> lines)
    {
        var linesArr = lines.ToArray();

        return new PixelLineCollection(linesArr);
    }
    
    private PixelLineCollection(PixelLine[] lines)
    {
        _lines = lines;
    }
}

public static partial class CollectionExtensions
{
    public static PixelLineCollection ToCollection(this IEnumerable<PixelLine> lines)
    {
        return PixelLineCollection.From(lines);
    }
}