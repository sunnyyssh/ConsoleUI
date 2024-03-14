namespace Sunnyyssh.ConsoleUI;

public delegate void ForceTakeFocusHandler(IFocusable sender);

public delegate void ForceLoseFocusHandler(IFocusable sender);

public interface IFocusable
{
    public bool IsWaitingFocus { get; } 
    public bool IsFocused { get; }
    protected internal void TakeFocus();
    protected internal void LoseFocus();
    protected internal bool HandlePressedKey(ConsoleKeyInfo keyInfo);
    protected internal event ForceTakeFocusHandler ForceTakeFocus;
    protected internal event ForceLoseFocusHandler ForceLoseFocus;
}