// ReSharper disable InconsistentlySynchronizedField

using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

internal record FocusManagerOptions(
    FocusFlowSpecification Specification,
    bool FocusFlowLoop, 
    bool ChangeFocusWhenSingle,  
    bool ThrowOnNotFocusedHandling, 
    ConsoleKey? SpecialKey = null);

internal record FocusFlowEndedArgs;

internal delegate void FocusFlowEndedHandler(FocusFlowManager lostManager, FocusFlowEndedArgs args);

internal delegate void ForceManagerTakeFocusHandler(FocusFlowManager manager);

/// <summary>
/// Manager of the whole focus flow. It handles keys and delegates it to the focused <see cref="IFocusable"/>.
/// </summary>
internal sealed class FocusFlowManager
{
    /// <summary>
    /// Focus flow and handling keys is delegated to this manager.
    /// If current <see cref="IFocusable"/> is <see cref="IFocusManagerHolder"/> (<see cref="Wrapper"/> for example)
    /// then its <see cref="FocusFlowManager"/> is the successor of focus.
    /// </summary>
    private FocusFlowManager? _successor;

    private readonly FocusManagerOptions _options;

    private readonly FocusableChain _focusableChain;
    
    private readonly object _keyHandlingLockObject = new();

    /// <summary>
    /// Focused child.
    /// </summary>
    public IFocusable? FocusedItem => _focusableChain.FocusedItem;
    
    /// <summary>
    /// Indicates if this instance should handle all keys on its own.
    /// </summary>
    public bool OverridesFlow { get; }

    /// <summary>
    /// Indicastes if any child is waiting for focus at this moment.
    /// </summary>
    public bool HasWaitingFocusable => _focusableChain.HasWaiting;
    
    public int ChildrenCount => _focusableChain.Count();

    public IFocusable[] Children => _focusableChain.ToArray();

    /// <summary>
    /// Indicates if this instance is handling focus now.
    /// </summary>
    public bool IsFocused { get; private set; }


    public bool TryGiveFocusTo(IFocusable focusable)
    {
        if (!_focusableChain.TrySetCurrentTo(focusable))
            return false;

        if (IsFocused)
        {
            if (_focusableChain.FocusedItem is {} from)
            {
                RemoveFocusFrom(from);
            }

            _focusableChain.SetFocusToCurrent();
                    
            GiveFocusTo(focusable);
        }

        return true;
    }
    
    /// <summary>
    /// Handles pressed key.
    /// </summary>
    /// <param name="args">Args of pressed key.</param>
    /// <exception cref="FocusFlowException">It's not focused.</exception>
    public void HandlePressedKey(KeyPressedArgs args)
    {
        lock (_keyHandlingLockObject)
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
                return;
            }

            // If it overrides flow then it should handle it on its own. 
            // Then it's neccessary to call it before own handling.
            if (_successor is not null && _successor.OverridesFlow)
            {
                _successor.HandlePressedKey(args);
                return;
            }

            if (IsKeyLose(args.KeyInfo.Key))
            {
                MoveNext();
                
                return;
            }

            // Check if key moves focus.
            if (TryGetNext(args.KeyInfo.Key, out var next)) 
            {
                if (_focusableChain.FocusedItem is {} from)
                {
                    RemoveFocusFrom(from);
                }

                _focusableChain.TrySetCurrentTo(next);
                _focusableChain.SetFocusToCurrent();
                    
                GiveFocusTo(next);
               
                return;
            }
        
            // Delegating focus if it doesn't override flow.
            if (_successor is {} successor)
            {
                successor.HandlePressedKey(args);
                return;
            }

            // If there is one focused it chould handle this key.
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

    // Checks in specification if key forces lose focus.
    private bool IsKeyLose(ConsoleKey key)
    {
        if (_focusableChain.Current is not {} current)
        {
            return false;
        }

        var spec = _options.Specification.Children[current];

        return spec.FocusLose.Contains(key);
    }

    // Tries to get next <see cref="IFocusable"/> if key should move focus to next according to the specification.
    private bool TryGetNext(ConsoleKey pressedKey, [NotNullWhen(true)] out IFocusable? next)
    {
        next = null;
        
        if (_focusableChain.FocusedItem is null)
        {
            return false;
        }
        
        if (!_options.Specification.Children.TryGetValue(_focusableChain.FocusedItem, out var spec))
        {
            return false;
        }

        return RecursiveGetNext(pressedKey, spec, out next);
    }

    // If child is not waiting for focus. Its specification is checked for next one by this key.
    // If it waits it is next.
    private bool RecursiveGetNext(ConsoleKey pressedKey, ChildSpecification from, [NotNullWhen(true)] out IFocusable? next)
    {
        next = null;
        
        if (!from.Flows.TryGetValue(pressedKey, out var possibleNext))
        {
            return false;
        }

        if (possibleNext.IsWaitingFocus)
        {
            next = possibleNext;
            return true;
        }
        
        var nextSpecification = _options.Specification.Children[possibleNext];
        
        return RecursiveGetNext(pressedKey, nextSpecification, out next);
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
    
    // Delegates focus flow handling to the enother manager.
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

    // Restores handling keys on its own.
    private void RestoreOwnManagement()
    {
        var pastSuccessor = _successor;
        _successor = null;
        if (pastSuccessor is not null)
        {
            pastSuccessor.LoseFocus();
        }
    }

    /// <summary>
    /// Takes focus. Makes current <see cref="IFocusable"/> take focus.
    /// </summary>
    /// <exception cref="FocusFlowException">It's already focused.</exception>
    public void TakeFocus()
    {
        if (IsFocused)
            throw new FocusFlowException("It's already focused.");
        IsFocused = true;
        _focusableChain.MoveNextWaitingFocus();
        _focusableChain.SetFocusToCurrent();
        if (_focusableChain.FocusedItem is {} focused)
        {
            GiveFocusTo(focused);
        }
    }
    
    /// <summary>
    /// Loses focus. Makes focused <see cref="IFocusable"/> lose focus.
    /// </summary>
    /// <exception cref="FocusFlowException">It's not focused.</exception>
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
    
    // ReSharper disable once UnusedMember.Local
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

    /// <summary>
    /// Creates an instance of <see cref="FocusFlowManager"/>.
    /// </summary>
    /// <param name="options">Specifies flow.</param>
    public FocusFlowManager(FocusManagerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        
        _options = options;
        OverridesFlow = options.Specification.OverridesFlow;
        
        _focusableChain = new FocusableChain(_options.FocusFlowLoop, options.Specification.Children.Keys);

        foreach (var focusable in options.Specification.Children.Keys)
        {
            SubscribeChildEvents(focusable);
        }
    }
}