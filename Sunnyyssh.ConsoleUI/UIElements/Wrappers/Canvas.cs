namespace Sunnyyssh.ConsoleUI;

public sealed class Canvas : Wrapper
{
    public bool AddChild(UIElement child, Position position)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        return AddChildProtected(child, position);
    }

    public bool RemoveChild(UIElement child)
    {
        ArgumentNullException.ThrowIfNull(child, nameof(child));

        return RemoveChildProtected(child);
    }

    #region Constructors
    
    public Canvas(int width, int height, ConsoleKey[] focusChangeKeys, OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : this(new Size(width, height), focusChangeKeys, overlappingPriority)
    { }

    public Canvas(int width, double heightRelation, ConsoleKey[] focusChangeKeys, OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : this(new Size(width, heightRelation), focusChangeKeys, overlappingPriority)
    { }

    public Canvas(double widthRelation, int height, ConsoleKey[] focusChangeKeys, OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : this(new Size(widthRelation, height), focusChangeKeys, overlappingPriority)
    { }

    public Canvas(double widthRelation, double heightRelation, ConsoleKey[] focusChangeKeys, OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : this(new Size(widthRelation, heightRelation), focusChangeKeys, overlappingPriority)
    { }

    public Canvas(Size size, ConsoleKey[] focusChangeKeys, 
        OverlappingPriority overlappingPriority = OverlappingPriority.Medium) 
        : base(size, overlappingPriority, focusChangeKeys, true)
    {
        
    }
    
    #endregion
}