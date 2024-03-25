namespace Sunnyyssh.ConsoleUI;

internal record Placement(int Left, int Top, int Width, int Height)
{
    public static bool AreTouched(Placement box1, Placement box2)
    {
        bool horizontalIntersected = 
            box1.Left < box2.Left + box2.Width 
            && box1.Left + box1.Width > box2.Left;
            
        bool verticalIntersected = 
            box1.Top < box2.Top + box2.Height 
            && box1.Top + box1.Height > box2.Top;
            
        bool horizontalTouched = 
            box1.Top == box2.Top + box2.Height 
            || box1.Top + box1.Height == box2.Top;
            
        bool verticalTouched = 
            box1.Left == box2.Left + box2.Width 
            || box1.Left + box1.Width == box2.Left;
            
        return horizontalTouched && horizontalIntersected 
               || verticalTouched && verticalIntersected;
    }

    public static bool AreIntersected(Placement box1, Placement box2)
    {
        bool horizontalIntersected = 
            box1.Left < box2.Left + box2.Width 
            && box1.Left + box1.Width > box2.Left;
        
        bool verticalIntersected = 
            box1.Top < box2.Top + box2.Height 
            && box1.Top + box1.Height > box2.Top;
        
        return horizontalIntersected && verticalIntersected;
    }
}