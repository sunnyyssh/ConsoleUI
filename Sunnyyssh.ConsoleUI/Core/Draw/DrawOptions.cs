namespace Sunnyyssh.ConsoleUI;

public sealed class DrawOptions
{
    public int Width { get; private init; }
    public int Height { get; private init; }

    public DrawOptions(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentException("width must be more than 0.");
        if (height <= 0)
            throw new ArgumentException("Height must be more than 0.");
        Width = width;
        Height = height;
    }
}