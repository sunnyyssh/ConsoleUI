// TODO make it thread-safe

namespace Sunnyyssh.ConsoleUI;

// TODO make it internal
public record FocusManagerOptions(ConsoleKey[] FocusChangeKeys, bool FocusFlowLoop, bool ThrowOnNotFocusedHandling = false);

// TODO make it internal
public record FocusFlowEndedArgs();

// TODO make it internal
public delegate void FocusFlowEndedHandler(FocusFlowManager lostManager, FocusFlowEndedArgs args);

// TODO make it internal
public delegate void ForceManagerTakeFocusHandler(FocusFlowManager manager);

// TODO make it internal
public sealed class FocusFlowManager
{
    private FocusFlowManager? _successor;

    private readonly FocusManagerOptions _options;

    private readonly ConcurrentChain<IFocusable> _chain = new();

    public IFocusable? Current => _chain.Current;

    public int ChildrenCount => _chain.Count;

    public IFocusable[] Children => _chain.ToArray();

    public bool IsFocused { get; private set; } = false;

    public void Add(IFocusable focusable)
    {
        _chain.Add(focusable);
        SubscribeChildEvents(focusable);
    }

    public bool TryRemove(IFocusable focusable)
    {
        bool removeResult = _chain.TryRemove(focusable);
        if (removeResult)
        {
            UnsubscribeChildEvents(focusable);
        }

        return removeResult;
    }

    public void InsertAt(int index, IFocusable focusable)
    {
        _chain.InsertAt(index, focusable);
        SubscribeChildEvents(focusable);
    }

    public bool TryInsertAfter(IFocusable afterThis, IFocusable newFocusable)
    {
        bool insertResult = _chain.TryInsertAfter(afterThis, newFocusable);
        if (insertResult)
        {
            SubscribeChildEvents(newFocusable);
        }

        return insertResult;
    }
    
    public bool TryInsertBefore(IFocusable beforeThis, IFocusable newFocusable)
    {
        bool insertResult = _chain.TryInsertBefore(beforeThis, newFocusable);
        if (insertResult)
        {
            SubscribeChildEvents(newFocusable);
        }

        return insertResult;
    }
    

    public void HandlePressedKey(KeyPressedArgs args)
    {
        if (!IsFocused)
        {
            if (_options.ThrowOnNotFocusedHandling)
                throw new FocusFlowException("It's not focused.");
            return;
        }
        
        if (_successor is {} successor)
        {
            successor.HandlePressedKey(args);
            return;
        }

        if (_chain.Current is {} current)
        {
            bool toKeepFocus = current.HandlePressedKey(args.KeyInfo);
            if (!toKeepFocus)
            {
                MoveNext();
            }
        }
    }

    private bool IsNeededToChangeFocus(KeyPressedArgs args)
    {
        return _options.FocusChangeKeys.Any(k => k == args.KeyInfo.Key);
    }

    private bool MoveNext()
    {
        IFocusable? past = _chain.Current;
        
        bool chainEnded = !_chain.MoveNext();
        
        if (chainEnded && !_options.FocusFlowLoop)
        {
            LoseFocus();
            return false;
        }
        
        IFocusable? newFocusable = _chain.Current;
        if (past is not null)
        {
            RemoveFocusFrom(past);
        }
        if (newFocusable is not null)
        {
            GiveFocusTo(newFocusable);
        }
        return true;
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
        if (_chain.Current is {} current)
        {
            GiveFocusTo(current);
        }
    }
    
    public void LoseFocus()
    {
        if (!IsFocused)
            throw new FocusFlowException("It's not focused.");
        IsFocused = false;
        if (_chain.Current is {} current)
        {
            RemoveFocusFrom(current);
        }
        FocusFlowEnded?.Invoke(this, new FocusFlowEndedArgs());
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
        var past = _chain.Current;
        bool changed = _chain.TrySetCurrentTo(sender);
        if (!changed)
            return;
        
        if (IsFocused)
        {
            if (past is not null)
            {
                RemoveFocusFrom(past);
            }
            GiveFocusTo(sender);
        }
        else
        {
            ForceTakeFocus?.Invoke(this);
        }
    }

    private void OnChildForceLoseFocus(IFocusable sender)
    {
        if (_chain.Current != sender)
            return;
        if (IsFocused)
        {
            MoveNext();
        }
        else
        {
            _chain.MoveNext();
        }
    }

    public FocusFlowManager(FocusManagerOptions options)
    {
        _options = options;
    }
}