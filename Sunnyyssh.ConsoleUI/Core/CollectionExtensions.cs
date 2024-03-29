namespace Sunnyyssh.ConsoleUI;

internal static partial class CollectionExtensions
{
    public static ChildrenCollection ToCollection(this IEnumerable<ChildInfo> children)
    {
        var orderedChildren = children.ToArray();
        
        return new ChildrenCollection(orderedChildren);
    }

    public static PixelLineCollection ToCollection(this IEnumerable<PixelLine> lines)
    {
        var linesArr = lines.ToArray();

        return new PixelLineCollection(linesArr);
    }

    public static PixelInfoCollection ToCollection(this IEnumerable<PixelInfo> pixels)
    {
        var pixelsArr = pixels.ToArray();

        return new PixelInfoCollection(pixelsArr);
    }
}