namespace Sunnyyssh.ConsoleUI;

public interface IFocusable
{
    void ForceEnterFocus(ForceEnterFocusOptions options);
    void ForceExitFocus(ForceExitFocusOptions options);
    event ForceEnteredFocusEventHandler ForceEnteredFocus;
    event ForceExitedFocusEventHandler ForceExitedFocus;
    void EnterFocus(EnterFocusOptions options);
    void ExitFocus(ExitFocusOptions options);
    bool IsWaitingFocus { get; }
    bool IsFocused { get; }
    
}