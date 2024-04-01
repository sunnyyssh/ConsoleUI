namespace Sunnyyssh.ConsoleUI;

public sealed class ChildSpecificationBuilder
{
    private readonly IFocusable _from;

    private readonly Dictionary<ConsoleKey, IFocusable> _flows = new();

    public ChildSpecificationBuilder AddFlow(IFocusable to, ConsoleKeyCollection keys)
    {
        ArgumentNullException.ThrowIfNull(to, nameof(to));
        ArgumentNullException.ThrowIfNull(keys, nameof(keys));

        foreach (var key in keys)
        {
            if (_flows.ContainsKey(key))
            {
                throw new ArgumentException($"{key} is already added.", nameof(keys));
            }

            _flows.Add(key, to);
        }
        
        return this;
    }

    public ChildSpecification Build()
    {
        IReadOnlyDictionary<ConsoleKey, IFocusable> readOnlyFlows = _flows;

        var result = new ChildSpecification(_from, readOnlyFlows);

        return result;
    }
    
    public ChildSpecificationBuilder(IFocusable from)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        _from = from;
    }
}