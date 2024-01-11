namespace Sunnyyssh.ConsoleUI;

public sealed class DrawState
{
    public PixelInfo[,] Pixels { get; private init; }
    public DrawState(PixelInfo[,] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));
        Pixels = pixels;
    }
}