namespace Sunnyyssh.ConsoleUI;

public sealed class RowTextChooser : RowChooser
{
    internal RowTextChooser(int width, int height, StackPanel optionsWrapper, 
        TextOptionElement[] orderedOptions, OptionChooserKeySet keySet, 
        bool canChooseOnlyOne, OverlappingPriority priority) 
    // TODO do something with immutability of this collections. (like orderedChildren)
        : base(width, height, optionsWrapper, orderedOptions, keySet, canChooseOnlyOne, priority) 
    {
        
    }
}