namespace Sunnyyssh.ConsoleUI;

public class RowChooser : WrapperBasedChooser<Grid>
{
    internal RowChooser(int width, int height, Grid optionsWrapper, IReadOnlyList<OptionElement> orderedOptions, 
        OptionChooserKeySet keySet, bool canChooseOnlyOne, OverlappingPriority priority) 
        : base(width, height, optionsWrapper, orderedOptions, keySet, canChooseOnlyOne, priority)
    {
        
    }
}