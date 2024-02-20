namespace Sunnyyssh.ConsoleUI;

internal interface IFocusManagerHolder : IFocusable
{
    protected internal FocusFlowManager GetFocusManager();
}