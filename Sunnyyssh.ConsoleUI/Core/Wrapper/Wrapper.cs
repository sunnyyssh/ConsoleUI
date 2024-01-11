namespace Sunnyyssh.ConsoleUI;

public class Wrapper : IKeyWaiter
{

    #region IKeyWatcher and IFocusable implementations
    
    private bool _isWaitingFocus;
    
    private bool _isFocused;
    
    private ForceEnteredFocusEventHandler? _forceEnteredFocusEventHandler;
    
    private ForceExitedFocusEventHandler? _forceExitedFocusEventHandler;
    
    void IFocusable.ForceEnterFocus(ForceEnterFocusOptions options)
    {
        throw new NotImplementedException();
    }

    void IFocusable.ForceExitFocus(ForceExitFocusOptions options)
    {
        throw new NotImplementedException();
    }

    event ForceEnteredFocusEventHandler IFocusable.ForceEnteredFocus
    {
        add => _forceEnteredFocusEventHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceEnteredFocusEventHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }

    event ForceExitedFocusEventHandler IFocusable.ForceExitedFocus
    {
        add => _forceExitedFocusEventHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceExitedFocusEventHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }

    void IFocusable.EnterFocus(EnterFocusOptions options)
    {
        throw new NotImplementedException();
    }

    void IFocusable.ExitFocus(ExitFocusOptions options)
    {
        throw new NotImplementedException();
    }

    bool IFocusable.IsWaitingFocus => _isWaitingFocus;

    bool IFocusable.IsFocused => _isFocused;

    void IKeyWaiter.OnKeyPressed(ConsoleKeyInfo key)
    {
        throw new NotImplementedException();
    }

    #endregion
    
    
}