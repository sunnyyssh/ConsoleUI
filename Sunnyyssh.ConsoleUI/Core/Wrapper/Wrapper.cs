using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public abstract class Wrapper : UIElement, IFocusManagerHolder, IElementContainer
{
    private readonly FocusFlowManager _focusFlowManager;

    private readonly LazyElementsField _lazyElementsField;

    private ForceTakeFocusHandler? _forceTakeFocusHandler;

    private ForceLoseFocusHandler? _forceLoseFocusHandler;

    public virtual bool IsWaitingFocus => _focusFlowManager.HasWaitingFocusable;

    protected bool IsInitialized => _lazyElementsField.IsInitialized;

    protected KeyValuePair<UIElement, Position>[] GetChildInfos => _lazyElementsField.IsInitialized
        ? _lazyElementsField.Field.GetChildInfos()
            .Select(ch => new KeyValuePair<UIElement, Position>(ch.Child, new Position(ch.Left, ch.Top)))
            .ToArray()
        : _lazyElementsField.GetEnqueuedChildren();
    
    public bool IsFocused { get; private set; }
    
    void IFocusable.TakeFocus()
    {
        IsFocused = true;
    }

    void IFocusable.LoseFocus()
    {
        IsFocused = false;
    }

    // It's never invoked because it's not provided by the idea of this class.
    // Watch FocusFlowManager.HandlePressedKey method 
    bool IFocusable.HandlePressedKey(ConsoleKeyInfo keyInfo)
    {
        throw new NotSupportedException();
    }

    event ForceTakeFocusHandler? IFocusable.ForceTakeFocus
    {
        add => _forceTakeFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceTakeFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }

    event ForceLoseFocusHandler? IFocusable.ForceLoseFocus
    {
        add => _forceLoseFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceLoseFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }

    FocusFlowManager IFocusManagerHolder.GetFocusManager()
    {
        return _focusFlowManager;
    }

    public bool Contains(UIElement child)
    {
        if (!_lazyElementsField.IsInitialized)
        {
            return _lazyElementsField.ContainsEnqueuedChild(child)
                   || _lazyElementsField.GetEnqueuedChildren().Any(
                       s => s.Key is IElementContainer container && container.Contains(child));
        }
        
        return _lazyElementsField.Field.Contains(child)
            || _lazyElementsField.Field.GetChildren().Any(
                s => s is IElementContainer container && container.Contains(child));
    }

    protected bool AddChildProtected(UIElement child, Position position)
    {
        if (child == this)
        {
            return false;
        }
        
        ChildInfo? childInfo = null;
        
        if (!_lazyElementsField.IsInitialized)
        {
            if (!_lazyElementsField.EnqueuePlaceChild(child, position))
                return false;
        }
        else
        {
            if (!_lazyElementsField.Field.TryPlaceChild(child, position, out childInfo))
                return false;
        }
                    
        child.RedrawElement += RedrawChild;
        
        if (child is IFocusable focusableChild)
        {
            _focusFlowManager.Add(focusableChild);
        }

        if (IsDrawn && childInfo is not null)
        {
            Redraw(GetChildState(childInfo));
        }

        return true;
    }

    protected bool RemoveChildProtected(UIElement child)
    {
        if (!_lazyElementsField.IsInitialized)
            return _lazyElementsField.RemoveEnqueued(child);

        if (!_lazyElementsField.Field.TryGetChild(child, out var childInfo))
            return false;

        child.RedrawElement -= RedrawChild;
        if (child is IFocusable focusableChild)
        {
            _ = _focusFlowManager.TryRemove(focusableChild);
        }

        if (IsDrawn)
        {
            EraseChild(childInfo);
        }

        return _lazyElementsField.Field.TryRemoveChild(child);
    }

    private void EraseChild(ChildInfo childInfo)
    {
        var erasingState = childInfo.CreateErasingState()
            .Shift(childInfo.Left, childInfo.Top);
        
        Redraw(erasingState);
        
        childInfo.Child.OnRemove();
    }

    protected override DrawState CreateDrawState(int width, int height)
    {
        if (!_lazyElementsField.IsInitialized)
        {
            _lazyElementsField.Initialize(width, height);
        }

        return GetChildrenCombinedState();
    }

    private DrawState GetChildrenCombinedState()
    {
        if (!_lazyElementsField.IsInitialized)
        {
            return DrawState.Empty;
        }

        var childrenStates = _lazyElementsField.Field.GetChildInfos()
            .Select(GetChildState)
            .ToArray();

        return DrawState.Combine(childrenStates);
    }

    private DrawState GetChildState(ChildInfo child)
    {
        child.Child.RequestDrawState(new DrawOptions());
        var result = child.TransformState();

        child.Child.OnDraw();
        
        return result;
    }
    
    private void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        if (!_lazyElementsField.IsInitialized)
            return;
        if (!_lazyElementsField.Field.TryGetChild(child, out var childInfo))
            return;

        var resultState = childInfo.TransformState();
        
        Redraw(resultState);
    }
    
    protected Wrapper(int width, int height, OverlappingPriority overlappingPriority, ConsoleKey[] focusChangeKeys, bool allowOverlapping) 
        : base(width, height, overlappingPriority)
    {
        ArgumentNullException.ThrowIfNull(focusChangeKeys, nameof(focusChangeKeys));
        
        _lazyElementsField = new LazyElementsField(allowOverlapping);
        
        FocusManagerOptions focusManagerOptions = new(
            focusChangeKeys,
            // It's not provided to loop focus flow
            // because the wrapper must lose focus when all wrapper's IFocusable went throgh focus
            false,
            false,
            true);
        _focusFlowManager = new FocusFlowManager(focusManagerOptions);

        _focusFlowManager.FocusFlowEnded += OnManagerFocusFlowEnded;
        _focusFlowManager.ForceTakeFocus += OnManagerForceTakeFocus;
    }

    private void OnManagerForceTakeFocus(FocusFlowManager manager)
    {
        _forceTakeFocusHandler?.Invoke(this);
    }

    private void OnManagerFocusFlowEnded(FocusFlowManager lostManager, FocusFlowEndedArgs args)
    {
        _forceLoseFocusHandler?.Invoke(this);
    }
}