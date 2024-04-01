namespace Sunnyyssh.ConsoleUI;

// Maybe it should be not Default but Empty or Bare
internal class DefaultApplication : Application
{
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

    private protected override void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        var childInfo = Children.SingleOrDefault(ch => ch.Child == child);

        if (childInfo is null)
            return;
        
        var resultDrawState = childInfo.TransformState(); // TODO it's bad.
        
        Drawer.EnqueueRequest(resultDrawState); 
    }

    public DefaultApplication(ApplicationSettings settings, ChildrenCollection orderedChildren, FocusFlowSpecification focusFlowSpecification) 
        : base(settings, orderedChildren, focusFlowSpecification)
    {
        // DefaultApplication doesn't present anything additional to Application implementation.
    }
}