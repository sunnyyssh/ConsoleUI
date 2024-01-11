namespace Sunnyyssh.ConsoleUI;

public interface IKeyWaiter : IFocusable
{
    void OnKeyPressed(ConsoleKeyInfo key);
}