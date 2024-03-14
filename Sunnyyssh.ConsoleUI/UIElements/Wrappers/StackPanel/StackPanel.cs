namespace Sunnyyssh.ConsoleUI;

public class StackPanel : Wrapper
{
    public Orientation Orientation { get; }

    public bool AddChild(UIElement element)
    {
        return AddChild(element, 0);
    }
    
    public bool AddChild(UIElement element, int space)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        if (space < 0)
            throw new ArgumentOutOfRangeException(nameof(space), space, "space should be non-negative.");

        throw new NotImplementedException(); // NotImplementedException
    }

    public StackPanel(Size size, Orientation orientation, ConsoleKey[] focusChangeKeys, OverlappingPriority overlappingPriority = OverlappingPriority.Medium)
        : base(size, overlappingPriority, focusChangeKeys, false)
    {
        Orientation = orientation;
    }
}