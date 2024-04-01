namespace Sunnyyssh.ConsoleUI;

public sealed class FocusFlowSpecification
{
    public bool OverridesFlow { get; }
    
    public IReadOnlyDictionary<IFocusable, ChildSpecification> Children { get; }

    internal FocusFlowSpecification(IReadOnlyDictionary<IFocusable, ChildSpecification> children, bool overridesFlow)
    {
        Children = children;
        OverridesFlow = overridesFlow;
    }
}

