namespace Sunnyyssh.ConsoleUI;

public class StackPanel : Wrapper
{
    public Orientation Orientation { get; }

    internal StackPanel(int width, int height, ChildrenCollection orderedChildren, Orientation orientation, ConsoleKeyCollection focusChangeKeys, OverlappingPriority overlappingPriority = OverlappingPriority.Medium)
        : base(width, height, orderedChildren, focusChangeKeys, overlappingPriority)
    {
        Orientation = orientation;
    }
}