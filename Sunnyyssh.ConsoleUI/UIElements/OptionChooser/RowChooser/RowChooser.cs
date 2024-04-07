using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public class RowChooser : WrapperBasedChooser<Grid>
{
    internal RowChooser(int width, int height, Grid optionsWrapper, ImmutableList<OptionElement> orderedOptions, 
        OptionChooserKeySet keySet, bool canChooseOnlyOne, OverlappingPriority priority) 
        : base(width, height, optionsWrapper, orderedOptions, keySet, canChooseOnlyOne, priority)
    {
        
    }
}