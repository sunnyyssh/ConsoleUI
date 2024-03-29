namespace Sunnyyssh.ConsoleUI;

public sealed class Canvas : Wrapper
{
    internal Canvas(int width, int height, ConsoleKey[] focusChangeKeys, ChildrenCollection orderedChildren,
        OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : base(width, height, orderedChildren, focusChangeKeys, overlappingPriority)
    { }
}