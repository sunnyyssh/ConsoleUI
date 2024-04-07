namespace Sunnyyssh.ConsoleUI;

public sealed class RowTextChooser : RowChooser
{
    internal RowTextChooser(int width, int height, Grid optionsWrapper, 
        IReadOnlyList<TextOptionElement> orderedOptions, OptionChooserKeySet keySet, 
        bool canChooseOnlyOne, OverlappingPriority priority) 
        : base(width, height, optionsWrapper, orderedOptions, keySet, canChooseOnlyOne, priority) 
    {
        
    }
}