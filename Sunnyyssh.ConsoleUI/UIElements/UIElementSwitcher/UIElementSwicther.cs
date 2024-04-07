using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public sealed class UIElementSwitcher : Wrapper, IFocusable
{
    public IReadOnlyList<Canvas> PresentationStates { get; }

    public int StateCount => PresentationStates.Count;
    
    public int CurrentStateIndex { get; private set; }

    public void SetCurrentStateTo(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= PresentationStates.Count)
            throw new ArgumentOutOfRangeException(nameof(stateIndex), stateIndex, null);

        if (stateIndex == CurrentStateIndex)
            return;

        int lastIndex = CurrentStateIndex;
        CurrentStateIndex = stateIndex;
        
        PresentationStates[lastIndex].IsWaitingFocus = false;
        PresentationStates[CurrentStateIndex].IsWaitingFocus = true;
        
        TryGiveFocusTo(PresentationStates[CurrentStateIndex]);
        
        if (IsDrawn)
        {
            if (PresentationStates[lastIndex].IsDrawn)
            {
                PresentationStates[lastIndex].OnRemove();
            }
            
            Redraw(CreateDrawState());
        }
    }
    
    protected override DrawState CreateDrawState()
    {
        if (!PresentationStates[CurrentStateIndex].IsDrawn)
        {
            OnDraw();
        }
        
        return PresentationStates[CurrentStateIndex].RequestDrawState(new DrawOptions());
    }

    private static ImmutableList<ChildInfo> ToLeftTopChildren(IReadOnlyList<UIElement> elements)
    {
        return elements
            .Select(el => new ChildInfo(el, 0, 0))
            .ToImmutableList();
    }

    private void RedrawCanvasState(UIElement sender, RedrawElementEventArgs args)
    {
        Redraw(sender.CurrentState!);
    }

    private void PrepareStates(IReadOnlyList<Canvas> presentationStates)
    {
        foreach (var canvasState in presentationStates)
        {
            canvasState.RedrawElement += RedrawCanvasState;

            canvasState.IsWaitingFocus = false;
        }

        presentationStates[CurrentStateIndex].IsWaitingFocus = true;
    }

    internal UIElementSwitcher(int width, int height, ImmutableList<Canvas> presentationStates, 
        FocusFlowSpecification excludingFocusSpecification,OverlappingPriority priority) 
        : base(width, height, ToLeftTopChildren(presentationStates), excludingFocusSpecification, priority)
    {
        ArgumentNullException.ThrowIfNull(presentationStates, nameof(presentationStates));

        if (presentationStates.IsEmpty)
            throw new ArgumentException("There must be at least one state.", nameof(presentationStates));

        PresentationStates = presentationStates;
        PrepareStates(presentationStates);
    }
}