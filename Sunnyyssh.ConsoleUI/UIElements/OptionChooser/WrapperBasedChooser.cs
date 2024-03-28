namespace Sunnyyssh.ConsoleUI;

public abstract class WrapperBasedChooser<TWrapper> : OptionChooser
    where TWrapper : Wrapper
{
    protected TWrapper OptionsWrapper { get; }
    
    protected override DrawState CreateDrawState(int width, int height)
    {
        return OptionsWrapper.RequestDrawState(DrawOptions.Empty);
    }

    private void RedrawWrapper(UIElement sender, RedrawElementEventArgs args)
    {
        Redraw(CreateDrawState(Width, Height));
    }

    public void SubscribeWrapper(TWrapper wrapper)
    {
        wrapper.RedrawElement += RedrawWrapper;
    }

    protected WrapperBasedChooser(int width, int height, TWrapper optionsWrapper, 
        OptionElement[] orderedOptions, OptionChooserKeySet keySet, bool canChooseOnlyOne, OverlappingPriority priority) 
        : base(width, height, orderedOptions, keySet, canChooseOnlyOne, priority)
    {
        OptionsWrapper = optionsWrapper;

        SubscribeWrapper(optionsWrapper);
    }
}