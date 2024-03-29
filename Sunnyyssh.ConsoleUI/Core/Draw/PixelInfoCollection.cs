using System.Collections;

namespace Sunnyyssh.ConsoleUI;

public sealed class PixelInfoCollection : IReadOnlyList<PixelInfo>
{
    public static readonly PixelInfoCollection Empty = new(Array.Empty<PixelInfo>());
    
    private readonly IReadOnlyList<PixelInfo> _pixels;

    public IEnumerator<PixelInfo> GetEnumerator() => _pixels.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _pixels.GetEnumerator();

    public int Count => _pixels.Count;

    public PixelInfo this[int index] => _pixels[index];

    public static PixelInfoCollection From(IEnumerable<PixelInfo> pixels)
    {
        var pixelsArr = pixels.ToArray();

        return new PixelInfoCollection(pixelsArr);
    }
    
    private PixelInfoCollection(PixelInfo[] pixels)
    {
        _pixels = pixels;
    }
}

public static partial class CollectionExtensions
{
    public static PixelInfoCollection ToCollection(this IEnumerable<PixelInfo> pixels)
    {
        return PixelInfoCollection.From(pixels);
    }
}