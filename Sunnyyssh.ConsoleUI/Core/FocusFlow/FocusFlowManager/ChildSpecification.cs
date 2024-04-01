namespace Sunnyyssh.ConsoleUI;

public sealed class ChildSpecification
{
    public IFocusable From { get; }
    
    public IReadOnlyDictionary<ConsoleKey, IFocusable> Flows { get; }
    
    public ConsoleKeyCollection FocusLose { get; }

    internal ChildSpecification(IFocusable from, IReadOnlyDictionary<ConsoleKey, IFocusable> flows, ConsoleKeyCollection focusLose)
    {
        From = from;
        Flows = flows;
        FocusLose = focusLose;
    }
}