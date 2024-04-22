// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// <see cref="Wrapper"/> that can place <see cref="UIElement"/>'s in a row (like a stack).
/// </summary>
public sealed class StackPanel : CompositionWrapper
{
    public Orientation Orientation { get; }

    internal StackPanel(int width, int height, ImmutableList<ChildInfo> orderedChildren, Orientation orientation, 
        FocusFlowSpecification focusFlowSpecification, OverlappingPriority overlappingPriority = OverlappingPriority.Medium)
        : base(width, height, orderedChildren, orderedChildren, focusFlowSpecification, overlappingPriority)
    {
        Orientation = orientation;
    }
}