// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// <see cref="UIElement"/> that consists of other <see cref="UIElement"/> instances.
/// </summary>
public abstract class Wrapper : UIElement, IFocusManagerHolder
{
    private readonly FocusFlowManager _focusFlowManager;

    private ForceTakeFocusHandler? _forceTakeFocusHandler;

    private ForceLoseFocusHandler? _forceLoseFocusHandler;

    /// <summary>
    /// Whether it waits for focus.
    /// </summary>
    public virtual bool IsWaitingFocus { get; set; } = true;
    
    bool IFocusable.IsWaitingFocus => IsWaitingFocus && _focusFlowManager.HasWaitingFocusable;

    /// <summary>
    /// <see cref="UIElement"/> children this instance consists of.
    /// </summary>
    public IReadOnlyList<ChildInfo> Children { get; }

    /// <summary>
    /// Whether it is focused now. 
    /// </summary>
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

    /// <summary>
    /// Whether this instance contains <see cref="child"/>.
    /// </summary>
    /// <param name="child"><see cref="UIElement"/> instance to check for being a child.</param>
    /// <returns>True if contains.</returns>
    public bool Contains(UIElement child)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));
        
        return Children.Any(ch => ch.Child == child);
    }

    protected bool TryGiveFocusTo(IFocusable focusable) => _focusFlowManager.TryGiveFocusTo(focusable);
    
    private void OnManagerForceTakeFocus(FocusFlowManager manager)
    {
        _forceTakeFocusHandler?.Invoke(this);
    }

    private void OnManagerFocusFlowEnded(FocusFlowManager lostManager, FocusFlowEndedArgs args)
    {
        _forceLoseFocusHandler?.Invoke(this);
    }

    protected internal override void OnDraw()
    {
        base.OnDraw();
        foreach (var child in Children)
        {
            if (!child.Child.IsDrawn)
            {
                child.Child.OnDraw();
            }
        }
    }

    protected internal override void OnRemove()
    {
        base.OnRemove();
        foreach (var child in Children)
        {
            if (child.Child.IsDrawn)
            {
                child.Child.OnRemove();
            }
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="Wrapper"/>.
    /// </summary>
    /// <param name="width">The absolute width.</param>
    /// <param name="height">The absolute height.</param>
    /// <param name="orderedChildren">Collection of <see cref="UIElement"/> children what this instance consists of.</param>
    /// <param name="excludingFocusSpecification">The specification of focus flow.</param>
    /// <param name="overlappingPriority">Overlapping priority.</param>
    protected Wrapper(int width, int height, 
        ImmutableList<ChildInfo> orderedChildren, FocusFlowSpecification excludingFocusSpecification, 
        OverlappingPriority overlappingPriority)
        : base(width, height, overlappingPriority)
    {
        ArgumentNullException.ThrowIfNull(orderedChildren, nameof(orderedChildren));
        ArgumentNullException.ThrowIfNull(excludingFocusSpecification, nameof(excludingFocusSpecification));

        var focusManagerOptions = new FocusManagerOptions(
            excludingFocusSpecification,
            // It's not provided to loop focus flow
            // because the wrapper must lose focus when all wrapper's IFocusable went throgh focus
            false,
            false,
            true);
        
        _focusFlowManager = new FocusFlowManager(focusManagerOptions);

        _focusFlowManager.FocusFlowEnded += OnManagerFocusFlowEnded;
        _focusFlowManager.ForceTakeFocus += OnManagerForceTakeFocus;

        Children = orderedChildren;
    }
}