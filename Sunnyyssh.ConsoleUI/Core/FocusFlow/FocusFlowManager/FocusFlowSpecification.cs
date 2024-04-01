namespace Sunnyyssh.ConsoleUI;

public sealed class FocusFlowSpecification
{
    public IReadOnlyDictionary<IFocusable, ChildSpecification> Children { get; }

    internal FocusFlowSpecification(IReadOnlyDictionary<IFocusable, ChildSpecification> children)
    {
        Children = children;
    }
}

