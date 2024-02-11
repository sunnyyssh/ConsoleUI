namespace Sunnyyssh.ConsoleUI;

// Maybe it should be not Default but Empty or Bare
internal class DefaultUIManager : UIManager
{
    public DefaultUIManager(UIManagerSettings settings) : base(settings)
    {
        // DefaultUIManager doesn't present anything additional to UIManager implementation.
    }

    private protected override void Draw()
    {
        var childInfos = ElementsField.GetChildInfos();
        foreach (var t in childInfos)
        {
            DrawChild(t);
        }
    }

    private protected override void DrawChild(ChildInfo child)
    {
        DrawOptions drawOptions = new(child.Width, child.Height);
        var drawState = child.Child.RequestDrawState(drawOptions);
        child.Child.OnDraw();
        
        HandleStateDrawing(child, drawState);
    }

    private protected override void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        if (!ElementsField.TryGetChild(child, out var childInfo))
            return;
        
        var drawState = args.State;
        HandleStateDrawing(childInfo, drawState);
    }

    private void HandleStateDrawing(ChildInfo childInfo, DrawState drawState)
    {
        var rowDrawState = drawState.ToInternal(childInfo.Left, childInfo.Top);
        
        // If some elements overlap current we should handle it.
        var resultDrawState = childInfo.SubtractStateWithOverlapping(rowDrawState);
        
        Drawer.EnqueueRequest(resultDrawState);
        
        // We should renew previous state.
        childInfo.PreviousState = rowDrawState;
    }
}