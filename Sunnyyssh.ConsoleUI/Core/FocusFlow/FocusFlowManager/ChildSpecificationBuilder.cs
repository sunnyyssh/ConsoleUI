// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Creates an instance of <see cref="ChildSpecification"/>. It gives an opportunity to add flows and <see cref="IFocusable"/> children.
/// </summary>
public sealed class ChildSpecificationBuilder
{
    private readonly IFocusable _from;

    private readonly Dictionary<ConsoleKey, IFocusable> _flows = new();

    private ImmutableList<ConsoleKey> _loseKeys = ImmutableList<ConsoleKey>.Empty;

    /// <summary>
    /// Adds flow to another child with specified keys.
    /// </summary>
    /// <param name="to">Child to flow focus to.</param>
    /// <param name="keys">Keys indicating focus should flow.</param>
    /// <returns>Same instance of <see cref="ChildSpecificationBuilder"/> to chain calls.</returns>
    /// <exception cref="ArgumentException">Key is already added.</exception>
    public ChildSpecificationBuilder AddFlow(IFocusable to, ImmutableList<ConsoleKey> keys)
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

    /// <summary>
    /// Adds focus lose keys.
    /// </summary>
    /// <param name="keys">Keys indicating that current <see cref="FocusFlowManager"/> should lose focus.</param>
    /// <returns>Same instance of <see cref="ChildSpecificationBuilder"/> to chain calls.</returns>
    public ChildSpecificationBuilder AddLoseFocus(ImmutableList<ConsoleKey> keys)
    {
        _loseKeys = keys.Union(_loseKeys).ToImmutableList();

        return this;
    }

    /// <summary>
    /// Builds <see cref="ChildSpecification"/>.
    /// </summary>
    /// <returns>Created instance of <see cref="ChildSpecification"/> with parameters that are given earlier.</returns>
    public ChildSpecification Build()
    {
        IReadOnlyDictionary<ConsoleKey, IFocusable> readOnlyFlows = _flows;

        var result = new ChildSpecification(_from, readOnlyFlows, _loseKeys);

        return result;
    }

    /// <summary>
    /// Creates <see cref="ChildSpecificationBuilder"/> on specified focusable child.
    /// </summary>
    /// <param name="from"></param>
    public ChildSpecificationBuilder(IFocusable from)
    {
        ArgumentNullException.ThrowIfNull(from, nameof(from));

        _from = from;
    }
}