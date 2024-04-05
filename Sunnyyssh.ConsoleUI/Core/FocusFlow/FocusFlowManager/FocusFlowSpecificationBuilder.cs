namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Creates an instance of <see cref="FocusFlowSpecification"/>.
/// </summary>
/// <example>
/// <code>
///
///  var specBuilder = new FocusFlowSpecificationBuilder(true);
///           
///  // Children that implement IFocusable and that are needed to take part in focus flow.
///  IFocusable[] focusables = ...;
///   
///   foreach (var focusable in focusables)
///  {
///      specBuilder.Add(focusable);
///  }
///           
///  for (int i = 0; i &lt; focusables.Length - 1; i++)
///  {
///      specBuilder.AddFlow(focusables[i], focusables[i + 1], _settings.FocusChangeKeys);
///  }
///   
///  // Cycle flow.
///  specBuilder.AddFlow(focusables[^1], focusables[0], _settings.FocusChangeKeys);
///   
///  var spec = specBuilder.Build();
/// </code>
/// </example>
public sealed class FocusFlowSpecificationBuilder
{
    private readonly Dictionary<IFocusable, ChildSpecificationBuilder> _children = new();

    /// <summary>
    /// Indicates if its <see cref="FocusFlowManager"/> should override focus flow.
    /// </summary>
    public bool OverridesFlow { get; }

    /// <summary>
    /// Adds child.
    /// </summary>
    /// <param name="child">Child to add.</param>
    /// <returns>Same instance of <see cref="FocusFlowSpecificationBuilder"/>.</returns>
    public FocusFlowSpecificationBuilder Add(IFocusable child)
    {
        var childSpecBuilder = new ChildSpecificationBuilder(child);
        
        _children.Add(child, childSpecBuilder);

        return this;
    }

    /// <summary>
    /// Adds focus lose keys to the child.
    /// </summary>
    /// <param name="from">Child losing focus on <see cref="keys"/></param>
    /// <param name="keys">Keys making lose focus.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Child hasn't been added.</exception>
    public FocusFlowSpecificationBuilder AddLoseFocus(IFocusable from, ConsoleKeyCollection keys)
    {
        if (!_children.TryGetValue(from, out var fromSpecBuilder))
        {
            throw new ArgumentException("Child hasn't been added.", nameof(from));
        }

        fromSpecBuilder.AddLoseFocus(keys);

        return this;
    }

    /// <summary>
    /// Adds flow from one child to another one.
    /// </summary>
    /// <param name="from">Child to flow from.</param>
    /// <param name="to">Child to flow to.</param>
    /// <param name="keys">Flow keys.</param>
    /// <returns>Same instance of <see cref="FocusFlowSpecificationBuilder"/></returns>
    /// <exception cref="ArgumentException"></exception>
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

    /// <summary>
    /// Creates <see cref="FocusFlowSpecification"/>.
    /// </summary>
    /// <returns>Created <see cref="FocusFlowSpecification"/> instance.</returns>
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

    /// <summary>
    /// Creates <see cref="FocusFlowSpecificationBuilder"/> instance.
    /// </summary>
    /// <param name="overridesFlow">Indicates if its &lt;see cref="FocusFlowManager"/&gt; should override focus flow.</param>
    public FocusFlowSpecificationBuilder(bool overridesFlow)
    {
        OverridesFlow = overridesFlow;
    }
}