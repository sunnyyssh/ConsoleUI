namespace Sunnyyssh.ConsoleUI;

public sealed class ChildSpecification
{
    public IFocusable From { get; }
    
    public IReadOnlyDictionary<ConsoleKey, IFocusable> Flows { get; }

    internal ChildSpecification(IFocusable from, IReadOnlyDictionary<ConsoleKey, IFocusable> flows)
    {
        From = from;
        Flows = flows;
    }
}