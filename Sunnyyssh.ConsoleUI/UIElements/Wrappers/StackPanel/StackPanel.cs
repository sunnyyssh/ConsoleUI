using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public class StackPanel : CompositionWrapper
{
    public Orientation Orientation { get; }

    internal StackPanel(int width, int height, ImmutableList<ChildInfo> orderedChildren, Orientation orientation, 
        FocusFlowSpecification focusFlowSpecification, OverlappingPriority overlappingPriority = OverlappingPriority.Medium)
        : base(width, height, orderedChildren, orderedChildren, focusFlowSpecification, overlappingPriority)
    {
        Orientation = orientation;
    }
}