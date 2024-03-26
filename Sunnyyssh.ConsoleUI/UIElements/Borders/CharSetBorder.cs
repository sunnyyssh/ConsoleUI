namespace Sunnyyssh.ConsoleUI;

internal class CharSetBorder : Border
{
    public CharSetBorder(int width, int height, BorderCharSet charSet, Color color, OverlappingPriority priority) 
        : base(width, height, charSet, color, priority)
    { }
}