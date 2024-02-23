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
        child.Child.OnDraw();
        DrawOptions drawOptions = new(child.Width, child.Height);
        var drawState = child.Child.RequestDrawState(drawOptions);
        HandleStateDrawing(child, drawState);
    }

    private protected override void EraseChild(ChildInfo child)
    {
        var erasingState = child.CreateErasingState();
        Drawer.EnqueueRequest(erasingState);
        child.Child.OnRemove();
    }

    private protected override void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        if (!ElementsField.TryGetChild(child, out var childInfo))
            return;
        
        HandleStateDrawing(childInfo, child.CurrentState!); // TODO it's bad.
    }

    private void HandleStateDrawing(ChildInfo childInfo, DrawState drawState)
    {
        var rowDrawState = drawState.Shift(childInfo.Left, childInfo.Top);

        var resultDrawState = childInfo.TransformState();
        
        Drawer.EnqueueRequest(resultDrawState);
    }
}