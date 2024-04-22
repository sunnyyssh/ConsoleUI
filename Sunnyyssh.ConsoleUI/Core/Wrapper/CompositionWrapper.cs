// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public abstract class CompositionWrapper : Wrapper
{
    private readonly IReadOnlyList<ChildInfo> _compositionChildren;
    

    protected override DrawState CreateDrawState()
    {
        return GetChildrenCombinedState();
    }

    private DrawState GetChildrenCombinedState()
    {
        var childrenStates = _compositionChildren
            .Select(RequestChildState)
            .ToArray();

        return DrawState.Combine(childrenStates);
    }

    private DrawState RequestChildState(ChildInfo child)
    {
        child.Child.RequestDrawState(new DrawOptions());
        
        var result = child.TransformState();

        // It's neccessary to invoke it on drawing.
        child.Child.OnDraw();
        
        return result;
    }

    private void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        var childInfo = Children.SingleOrDefault(ch => ch.Child == child);

        if (childInfo is null)
            return;

        var resultState = childInfo.TransformState();
        
        Redraw(resultState);
    }

    // ReSharper disable once UnusedMember.Local
    private void EraseChild(ChildInfo childInfo)
    {
        ArgumentNullException.ThrowIfNull(childInfo, nameof(childInfo));

        var erasingState = childInfo.CreateErasingState()
            .Shift(childInfo.Left, childInfo.Top);
        
        Redraw(erasingState);
        
        childInfo.Child.OnRemove();
    }

    protected CompositionWrapper(int width, int height, ImmutableList<ChildInfo> orderedChildren,
        ImmutableList<ChildInfo> compositionChildren, FocusFlowSpecification focusFlowSpecification, OverlappingPriority overlappingPriority) 
        : base(width, height, orderedChildren, focusFlowSpecification, overlappingPriority)
    {
        ArgumentNullException.ThrowIfNull(compositionChildren, nameof(compositionChildren));

        _compositionChildren = compositionChildren;
        
        foreach (var child in _compositionChildren)
        {
            child.Child.RedrawElement += RedrawChild;
        }
    }
}