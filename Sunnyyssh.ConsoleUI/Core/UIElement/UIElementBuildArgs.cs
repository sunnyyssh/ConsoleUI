namespace Sunnyyssh.ConsoleUI;

public class UIElementBuildArgs
{
    public int Width { get; }

    public int Height { get; }

    public UIElementBuildArgs(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, null);
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, null);
        
        Width = width;
        Height = height;
    }
}