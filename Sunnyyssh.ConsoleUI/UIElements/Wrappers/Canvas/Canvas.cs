// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// <see cref="Wrapper"/> that can place <see cref="UIElement"/>'s at the specific position.
/// </summary>
public sealed class Canvas : CompositionWrapper
{

    internal Canvas(int width, int height, FocusFlowSpecification focusFlowSpecification, ImmutableList<ChildInfo> orderedChildren,
        OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : base(width, height, orderedChildren, orderedChildren, focusFlowSpecification, overlappingPriority)
    { }
}