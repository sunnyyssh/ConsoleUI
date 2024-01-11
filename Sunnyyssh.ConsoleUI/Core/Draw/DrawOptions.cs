namespace Sunnyyssh.ConsoleUI;

public sealed class DrawOptions
{
    public int Height { get; private init; }
    public int Width { get; private init; }

    public DrawOptions(int height, int width)
    {
        if (height <= 0)
            throw new ArgumentException("Height must be more than 0.");
        if (width <= 0)
            throw new ArgumentException("width must be more than 0.");
        Height = height;
        Width = width;
    }
}