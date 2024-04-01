namespace Sunnyyssh.ConsoleUI;

public sealed class FocusFlowSpecificationBuilder
{
    private readonly Dictionary<IFocusable, ChildSpecificationBuilder> _children = new();

    public bool OverridesFlow { get; }

    public FocusFlowSpecificationBuilder Add(IFocusable child)
    {
        var childSpecBuilder = new ChildSpecificationBuilder(child);
        
        _children.Add(child, childSpecBuilder);

        return this;
    }

    public FocusFlowSpecificationBuilder AddLoseFocus(IFocusable from, ConsoleKeyCollection keys)
    {
        if (!_children.TryGetValue(from, out var fromSpecBuilder))
        {
            throw new ArgumentException("Child hasn't been added.", nameof(from));
        }

        fromSpecBuilder.AddLoseFocus(keys);

        return this;
    }

    public FocusFlowSpecificationBuilder AddFlow(IFocusable from, IFocusable to, ConsoleKeyCollection keys)
    {
        if (!_children.TryGetValue(from, out var fromSpecBuilder))
        {
            throw new ArgumentException("Child hasn't been added.", nameof(from));
        }
        
        if (!_children.TryGetValue(to, out _))
        {
            throw new ArgumentException("Child hasn't been added.", nameof(from));
        }

        fromSpecBuilder.AddFlow(to, keys);

        return this;
    }

    public FocusFlowSpecification Build()
    {
        IReadOnlyDictionary<IFocusable, ChildSpecification> readOnlyChildren =
            _children
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Build());

        var result = new FocusFlowSpecification(readOnlyChildren, OverridesFlow);

        return result;
    }

    public FocusFlowSpecificationBuilder(bool overridesFlow)
    {
        OverridesFlow = overridesFlow;
    }
}