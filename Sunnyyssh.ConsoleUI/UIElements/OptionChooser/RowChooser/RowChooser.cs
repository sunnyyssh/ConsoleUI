namespace Sunnyyssh.ConsoleUI;

public class RowChooser : WrapperBasedChooser<StackPanel>
{
    internal RowChooser(int width, int height, StackPanel optionsWrapper, IReadOnlyList<OptionElement> orderedOptions, 
        OptionChooserKeySet keySet, bool canChooseOnlyOne, OverlappingPriority priority) 
        : base(width, height, optionsWrapper, orderedOptions, keySet, canChooseOnlyOne, priority)
    {
        
    }
}