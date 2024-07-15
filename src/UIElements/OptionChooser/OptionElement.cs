// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

public abstract class OptionElement : UIElement
{
    protected internal abstract void FocusOn();

    protected internal abstract void FocusOff();

    protected internal abstract void ChosenOn();

    protected internal abstract void ChosenOff();

    public abstract bool IsChosen { get; }
    
    public abstract bool IsFocused { get; }
    
    protected OptionElement(int width, int height) 
        : base(width, height, 
            // OverlappingPriority set to Medium because there are no matter if OptionChoosers will place them without overlapping.
            OverlappingPriority.Medium)
    { }
}