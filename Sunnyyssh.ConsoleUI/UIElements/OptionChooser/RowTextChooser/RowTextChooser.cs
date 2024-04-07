using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public sealed class RowTextChooser : RowChooser
{
    internal RowTextChooser(int width, int height, Grid optionsWrapper, 
        ImmutableList<TextOptionElement> orderedOptions, OptionChooserKeySet keySet, 
        bool canChooseOnlyOne, OverlappingPriority priority) 
        : base(width, height, optionsWrapper, orderedOptions.Cast<OptionElement>().ToImmutableList(), 
            keySet, canChooseOnlyOne, priority) 
    {
        
    }
}