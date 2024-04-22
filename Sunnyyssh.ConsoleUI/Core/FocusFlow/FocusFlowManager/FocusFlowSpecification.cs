// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Specification of focus flow.
/// </summary>
public sealed class FocusFlowSpecification
{
    /// <summary>
    /// Indicates if its <see cref="FocusFlowManager"/> should override focus flow.
    /// </summary>
    public bool OverridesFlow { get; }
    
    /// <summary>
    /// <see cref="IFocusable"/> Children and their <see cref="ChildSpecification"/> specifications.
    /// </summary>
    public IReadOnlyDictionary<IFocusable, ChildSpecification> Children { get; }
    
    internal FocusFlowSpecification(IReadOnlyDictionary<IFocusable, ChildSpecification> children, bool overridesFlow)
    {
        Children = children;
        OverridesFlow = overridesFlow;
    }
}

