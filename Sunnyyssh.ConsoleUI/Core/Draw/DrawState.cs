namespace Sunnyyssh.ConsoleUI;

public sealed class DrawState
{
    internal int? Top { get; private set; }
    internal int? Left { get; private set; }
    public PixelInfo[,] Pixels { get; private init; }
    public DrawState(PixelInfo[,] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));
        Pixels = pixels;
    }

    internal DrawState WithPosition(int left, int top)
    {
        DrawState result = ShallowCopy();
        result.Left = left;
        result.Top = top;
        return result;
    }

    internal DrawState ShallowCopy()
    {
        return new DrawState(Pixels)
        {
            Top = this.Top, 
            Left = this.Left,
        };
    }
}