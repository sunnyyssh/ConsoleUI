namespace Sunnyyssh.ConsoleUI;

// It must subscribe ForceTakeFocus event on its FocusFlowManager
public interface IFocusManagerHolder : IFocusable
{
    protected internal FocusFlowManager GetFocusManager();
}