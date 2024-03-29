// ReSharper disable InconsistentlySynchronizedField
namespace Sunnyyssh.ConsoleUI;

internal record FocusManagerOptions(
    ConsoleKeyCollection FocusChangeKeys, 
    bool FocusFlowLoop, 
    bool ChangeFocusWhenSingle,  
    bool ThrowOnNotFocusedHandling, 
    ConsoleKey? SpecialKey = null);

internal record FocusFlowEndedArgs;

internal delegate void FocusFlowEndedHandler(FocusFlowManager lostManager, FocusFlowEndedArgs args);

internal delegate void ForceManagerTakeFocusHandler(FocusFlowManager manager);

internal sealed class FocusFlowManager
{
    private FocusFlowManager? _successor;

    private readonly FocusManagerOptions _options;

    private readonly FocusableChain _focusableChain;
    
    private readonly object _lockObject = new();

    public IFocusable? FocusedItem => _focusableChain.FocusedItem;

    public bool HasWaitingFocusable => _focusableChain.HasWaiting;

    public int ChildrenCount => _focusableChain.Count();

    public IFocusable[] Children => _focusableChain.ToArray();

    public bool IsFocused { get; private set; }

    public void Add(IFocusable focusable)
    {
        ArgumentNullException.ThrowIfNull(focusable, nameof(focusable));
        
        lock (_lockObject)
        {
            _focusableChain.Add(focusable);
            if (IsFocused && _focusableChain.FocusedItem is null)
            {
                MoveNext();
            }
            SubscribeChildEvents(focusable);
        }
    }

    public bool TryRemove(IFocusable focusable)
    {
        ArgumentNullException.ThrowIfNull(focusable, nameof(focusable));

        lock (_lockObject)
        {
            var past = _focusableChain.FocusedItem ?? _focusableChain.Current;
            if (!_focusableChain.TryRemove(focusable))
            {
                return false;
            }
        
            if (past == focusable)
            {
                MoveNext();
            }
            UnsubscribeChildEvents(focusable);

            return true;
        }
    }

    public void InsertAt(int index, IFocusable focusable)
    {
        ArgumentNullException.ThrowIfNull(focusable, nameof(focusable));

        lock (_lockObject)
        {
            _focusableChain.InsertAt(index, focusable);
            SubscribeChildEvents(focusable);

            if (IsFocused && _focusableChain.FocusedItem is null)
            {
                MoveNext();
            }
        }
    }

    public bool TryInsertAfter(IFocusable afterThis, IFocusable newFocusable) 
    {
        ArgumentNullException.ThrowIfNull(afterThis, nameof(afterThis));
        ArgumentNullException.ThrowIfNull(newFocusable, nameof(newFocusable));

        lock (_lockObject)
        {
            if (!_focusableChain.TryInsertAfter(afterThis, newFocusable))
                return false;
        
            SubscribeChildEvents(newFocusable);
        
            if (IsFocused && _focusableChain.FocusedItem is null)
            {
                MoveNext();
            }

            return true;
        }
    }
    
    public bool TryInsertBefore(IFocusable beforeThis, IFocusable newFocusable) 
    {
        ArgumentNullException.ThrowIfNull(beforeThis, nameof(beforeThis));
        ArgumentNullException.ThrowIfNull(newFocusable, nameof(newFocusable));
        
        lock (_lockObject)
        {
            if (!_focusableChain.TryInsertBefore(beforeThis, newFocusable))
                return false;
        
            SubscribeChildEvents(newFocusable);
        
            if (IsFocused && _focusableChain.FocusedItem is null)
            {
                MoveNext();
            }

            return true;
        }
    }
    
    public void HandlePressedKey(KeyPressedArgs args)
    {
        lock (_lockObject)
        {
            if (!IsFocused)
            {
                if (_options.ThrowOnNotFocusedHandling)
                    throw new FocusFlowException("It's not focused.");
                return;
            }

            if (_options.SpecialKey == args.KeyInfo.Key)
            {
                SpecialKeyPressed?.Invoke(args);
            }
        
            if (_successor is {} successor)
            {
                successor.HandlePressedKey(args);
                return;
            }

            if (IsNeededToChangeFocus(args)) 
            {
                MoveNext();
                return;
            }

            if (_focusableChain.FocusedItem is {} focused)
            {
                bool toKeepFocus = focused.HandlePressedKey(args.KeyInfo);
                if (!toKeepFocus)
                {
                    MoveNext();
                }
            }
        }
    }

    private bool IsNeededToChangeFocus(KeyPressedArgs args)
    {
        return _options.FocusChangeKeys.Any(k => k == args.KeyInfo.Key);
    }

    // It goes to the next IFocusable which IsWaitingFocus == true
    // And it handles their focus change.
    private void MoveNext()
    {
        if (_focusableChain.IsEmpty)
            return;

        if (!IsFocused)
        {
            _ = _focusableChain.MoveNext();
            return;
        }

        if (_focusableChain.FocusedItem == _focusableChain.Current)
        {
            if (!_focusableChain.MoveNextWaitingFocus())
            {
                FocusFlowEnded?.Invoke(this, new FocusFlowEndedArgs());
                return;
            }

            if (_focusableChain.Current == _focusableChain.FocusedItem 
                && !_options.ChangeFocusWhenSingle
                // If current IFocusable is IFocusManagerHolder then it should handle focus change.
                // It's bad decision but it's not expensive.
                && _focusableChain.Current is not IFocusManagerHolder)
                return;
        }
        
        if (_focusableChain.FocusedItem is not null) 
            RemoveFocusFrom(_focusableChain.FocusedItem);
        
        if (_focusableChain.Current is not null)
        {
            GiveFocusTo(_focusableChain.Current);
            _focusableChain.SetFocusToCurrent();
        }
    }
    
    public event FocusFlowEndedHandler? FocusFlowEnded; 

    public event ForceManagerTakeFocusHandler? ForceTakeFocus; 
    
    private void GiveManagementTo(FocusFlowManager successor)
    {
        if (successor == this)
        {
            throw new FocusFlowException($"Focus manager can't give management to itself. {nameof(successor)} was same as this instance.");
        }

        var pastSuccessor = _successor;
        _successor = successor;
        if (pastSuccessor is not null)
        {
            pastSuccessor.LoseFocus();
        }

        _successor.TakeFocus();
    }

    private void RestoreOwnManagement()
    {
        var pastSuccessor = _successor;
        _successor = null;
        if (pastSuccessor is not null)
        {
            pastSuccessor.LoseFocus();
        }
    }

    public void TakeFocus()
    {
        if (IsFocused)
            throw new FocusFlowException("It's already focused.");
        IsFocused = true;
        _focusableChain.SetFocusToCurrent();
        if (_focusableChain.FocusedItem is {} focused)
        {
            GiveFocusTo(focused);
        }
    }
    
    public void LoseFocus()
    {
        if (!IsFocused)
            throw new FocusFlowException("It's not focused.");
        IsFocused = false;
        if (_focusableChain.FocusedItem is {} focused)
        {
            _focusableChain.LoseFocus();
            RemoveFocusFrom(focused);
        }
    }

    private void GiveFocusTo(IFocusable focusable)
    {
        focusable.TakeFocus();
        if (focusable is IFocusManagerHolder holder)
        {
            GiveManagementTo(holder.GetFocusManager());
        }
    }

    private void RemoveFocusFrom(IFocusable focusable)
    {
        if (focusable is IFocusManagerHolder)
        {
            RestoreOwnManagement();
        }
        focusable.LoseFocus();
    }

    private void SubscribeChildEvents(IFocusable child)
    {
        child.ForceLoseFocus += OnChildForceLoseFocus;
        child.ForceTakeFocus += OnChildForceTakeFocus;
    }
    
    private void UnsubscribeChildEvents(IFocusable child)
    {
        child.ForceLoseFocus -= OnChildForceLoseFocus;
        child.ForceTakeFocus -= OnChildForceTakeFocus;
    }

    private void OnChildForceTakeFocus(IFocusable sender)
    {
        if (!_focusableChain.TrySetCurrentTo(sender))
            return;
        
        if (IsFocused)
        {
            if (_focusableChain.FocusedItem is { } past)
            {
                RemoveFocusFrom(past);
            }

            _focusableChain.SetFocusToCurrent();
            GiveFocusTo(sender);
        }
        else
        {
            ForceTakeFocus?.Invoke(this);
        }
    }

    private void OnChildForceLoseFocus(IFocusable sender)
    {
        if (_focusableChain.FocusedItem != sender)
            return;
        if (IsFocused)
        {
            MoveNext();
        }
    }

    public event KeyPressedHandler? SpecialKeyPressed;

    public FocusFlowManager(FocusManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        
        _options = options;
        _focusableChain = new FocusableChain(_options.FocusFlowLoop);
    }
}