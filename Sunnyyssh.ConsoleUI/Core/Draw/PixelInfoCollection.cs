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

    internal PixelInfoCollection(PixelInfo[] pixels)
    {
        _pixels = pixels;
    }
}