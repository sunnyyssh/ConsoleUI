using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// The default implementation of <see cref="Application"/>.
/// </summary>
internal class DefaultApplication : Application
{
    /// <summary>
    /// <inheritdoc cref="Application.Draw"/>
    /// </summary>
    private protected override void Draw()
    {
        var combinedState =
            DrawState.Combine(
                Children
                    .Select(child =>
                        child.Child.RequestDrawState(new DrawOptions())
                            .Shift(child.Left, child.Top))
                    .ToArray()
            );

        foreach (var child in Children)
        {
            child.Child.OnDraw();
        }
        
        Drawer.EnqueueRequest(combinedState);
    }

    /// <summary>
    /// <inheritdoc cref="Application.RedrawChild"/>
    /// </summary>
    /// <param name="child">Child to redraw.</param>
    /// <param name="args">Redraw args.</param>
    private protected override void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        var childInfo = Children.SingleOrDefault(ch => ch.Child == child);

        if (childInfo is null)
            return;
        
        var resultDrawState = childInfo.TransformState();
        
        Drawer.EnqueueRequest(resultDrawState); 
    }

    /// <summary>
    /// Creates an instance of <see cref="DefaultApplication"/>.
    /// </summary>
    public DefaultApplication(ApplicationSettings settings, ImmutableList<ChildInfo> orderedChildren, FocusFlowSpecification focusFlowSpecification) 
        : base(settings, orderedChildren, focusFlowSpecification)
    {
        // DefaultApplication doesn't present anything additional to Application implementation.
    }
}