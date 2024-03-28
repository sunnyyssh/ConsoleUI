namespace Sunnyyssh.ConsoleUI.Grid;

public class Grid : Wrapper
{
    
    
    internal Grid(int width, int height, ChildInfo[] orderedChildren, 
        ConsoleKey[] focusChangeKeys, OverlappingPriority overlappingPriority) 
        : base(width, height, orderedChildren, focusChangeKeys, overlappingPriority)
    {
        
    }
}