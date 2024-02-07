namespace Sunnyyssh.ConsoleUI;

// Maybe it should be not Default but Empty or Bare
internal class DefaultUIManager : UIManager
{
    public DefaultUIManager(UIManagerSettings settings) : base(settings)
    {
        throw new NotImplementedException();
    }

    public override int BufferWidth => Drawer.BufferWidth;
    public override int BufferHeight => Drawer.BufferHeight;
    
    private protected override void Draw()
    {
        throw new NotImplementedException();
    }

    private protected override void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        throw new NotImplementedException();
    }
}