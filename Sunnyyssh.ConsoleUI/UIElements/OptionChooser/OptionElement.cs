namespace Sunnyyssh.ConsoleUI;

public abstract class OptionElement : UIElement
{
    public abstract void FocusOn();

    public abstract void FocusOff();

    public abstract void ChosenOn();

    public abstract void ChosenOff();

    public abstract bool IsChosen { get; }
    
    protected OptionElement(int width, int height, OverlappingPriority priority) : base(width, height, priority)
    { }
}