namespace Sunnyyssh.ConsoleUI;

public abstract class OptionElement : UIElement
{
    public abstract void FocusOn();

    public abstract void FocusOff();

    public abstract void ChosenOn();

    public abstract void ChosenOff();

    public abstract bool IsChosen { get; }
    
    public abstract bool IsFocused { get; }
    
    protected OptionElement(int width, int height) 
        : base(width, height, 
            // OverlappingPriority set to Medium because there are no matter if OptionChoosers will place them without overlapping.
            OverlappingPriority.Medium)
    { }
}