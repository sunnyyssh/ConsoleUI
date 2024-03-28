namespace Sunnyyssh.ConsoleUI;

public class StackPanel : Wrapper
{
    public Orientation Orientation { get; }

    internal StackPanel(int width, int height, ChildInfo[] orderedChildren, Orientation orientation, ConsoleKey[] focusChangeKeys, OverlappingPriority overlappingPriority = OverlappingPriority.Medium)
        : base(width, height, orderedChildren, focusChangeKeys, overlappingPriority)
    {
        Orientation = orientation;
    }
}