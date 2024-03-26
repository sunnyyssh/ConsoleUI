using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public abstract class Wrapper : UIElement, IFocusManagerHolder, IElementContainer
{
    private readonly FocusFlowManager _focusFlowManager;

    private ForceTakeFocusHandler? _forceTakeFocusHandler;

    private ForceLoseFocusHandler? _forceLoseFocusHandler;
    
    private readonly ChildInfo[] _orderedChildren;

    public virtual bool IsWaitingFocus => _focusFlowManager.HasWaitingFocusable;

    public ChildInfo[] Children => _orderedChildren.ToArray();

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
        return Children.Any(ch => ch.Child == child);
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
        return GetChildrenCombinedState();
    }

    private DrawState GetChildrenCombinedState()
    {
        var childrenStates = Children
            .Select(RequestChildState)
            .ToArray();

        return DrawState.Combine(childrenStates);
    }

    private DrawState RequestChildState(ChildInfo child)
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

        var childInfo = Children.SingleOrDefault(ch => ch.Child == child);

        if (childInfo is null)
            return;

        var resultState = childInfo.TransformState();
        
        Redraw(resultState);
    }

    private void OnManagerForceTakeFocus(FocusFlowManager manager)
    {
        _forceTakeFocusHandler?.Invoke(this);
    }

    private void OnManagerFocusFlowEnded(FocusFlowManager lostManager, FocusFlowEndedArgs args)
    {
        _forceLoseFocusHandler?.Invoke(this);
    }

    private void ValidateChildren(ChildInfo[] orderedChildren)
    {
        for (int i = 0; i < orderedChildren.Length; i++)
        {
            for (int j = i + 1; j < orderedChildren.Length; j++)
            {
                if (orderedChildren[i].Child == orderedChildren[j].Child)
                    throw new ChildPlacementException("Attempt to add two equal children occured.");
            }
        }
    }

    protected Wrapper(int width, int height, 
        ChildInfo[] orderedChildren, ConsoleKey[] focusChangeKeys, 
        OverlappingPriority overlappingPriority)
        : base(width, height, overlappingPriority)
    {
        ArgumentNullException.ThrowIfNull(orderedChildren, nameof(orderedChildren));
        ArgumentNullException.ThrowIfNull(focusChangeKeys, nameof(focusChangeKeys));

        var focusManagerOptions = new FocusManagerOptions(
            focusChangeKeys,
            // It's not provided to loop focus flow
            // because the wrapper must lose focus when all wrapper's IFocusable went throgh focus
            false,
            false,
            true);
        _focusFlowManager = new FocusFlowManager(focusManagerOptions);

        _focusFlowManager.FocusFlowEnded += OnManagerFocusFlowEnded;
        _focusFlowManager.ForceTakeFocus += OnManagerForceTakeFocus;

        ValidateChildren(orderedChildren);
        
        _orderedChildren = orderedChildren;

        foreach (var child in _orderedChildren)
        {
            child.Child.RedrawElement += RedrawChild;
        
            if (child.Child is IFocusable focusableChild)
            {
                _focusFlowManager.Add(focusableChild);
            }
        }
    }
}