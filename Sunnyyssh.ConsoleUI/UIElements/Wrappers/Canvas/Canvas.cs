namespace Sunnyyssh.ConsoleUI;

public sealed class Canvas : Wrapper
{
    public Canvas(int width, int height, ConsoleKey[] focusChangeKeys, ChildInfo[] orderedChildren,
        OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : base(width, height, orderedChildren, focusChangeKeys, overlappingPriority)
    { }
}