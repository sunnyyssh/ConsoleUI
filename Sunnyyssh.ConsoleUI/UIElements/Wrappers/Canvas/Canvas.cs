namespace Sunnyyssh.ConsoleUI;

public sealed class Canvas : CompositionWrapper
{
    internal Canvas(int width, int height, FocusFlowSpecification focusFlowSpecification, ChildrenCollection orderedChildren,
        OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : base(width, height, orderedChildren, orderedChildren, focusFlowSpecification, overlappingPriority)
    { }
}