// #define INTERNAL_TYPES_TESTS

using Sunnyyssh.ConsoleUI;

namespace Test;

public static class TestGenerator
{
    private static Random random = new Random();

    public static Color GenColor() => (Color)random.Next(5);

    public static char GenChar() => (char)random.Next((int)'a', (int)'z');
    
    public static PixelInfo GenPixelInfo()
    {
        if (random.NextDouble() < 0.1)
        {
            return new PixelInfo();
        }

        return new PixelInfo(GenChar(), GenColor(), GenColor());
    }

    public static PixelLine GenPixelLine(int left = -1, int top = -1, int length = -1)
    {
        int glength = length == -1 ? random.Next(5, 20) : length;
        int gtop = top == -1 ? random.Next(3) : top;
        int gleft = left == -1 ? random.Next(10) : left;
        PixelInfo[] pixels = Enumerable.Range(0, glength).Select(_ => GenPixelInfo()).ToArray();

        return new PixelLine(gleft, gtop, pixels);
    }

    public static PixelLine GenPixelLine(int left, int top, Color background, Color foreground, string text)
    {
        var pixels = Enumerable.Range(0, text.Length)
            .Select(i => new PixelInfo(text[i], background, foreground))
            .ToArray();
        return new PixelLine(left, top, pixels);
    }

#if INTERNAL_TYPES_TESTS
    public static InternalDrawState GenInternalDrawState()
    {
        return new InternalDrawState(
            Enumerable.Range(0, 5)
                .Select(i => GenPixelLine(top: i))
                .ToArray());
    }
#endif
}