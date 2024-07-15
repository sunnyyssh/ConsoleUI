// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Specification of child's (<see cref="IFocusable"/>'s) focus flow.
/// </summary>
public sealed class ChildSpecification
{
    /// <summary>
    /// Specifies from what focusable child focus flows.
    /// </summary>
    public IFocusable From { get; }
    
    /// <summary>
    /// Specifies flows to other children by specific keys.
    /// </summary>
    public IReadOnlyDictionary<ConsoleKey, IFocusable> Flows { get; }
    
    /// <summary>
    /// Keys indicating that current <see cref="FocusFlowManager"/> must lose focus.
    /// </summary>
    public IReadOnlyList<ConsoleKey> FocusLose { get; }

    /// <summary>
    /// Creates <see cref="ChildSpecification"/> instance.
    /// </summary>
    /// <param name="from">Child from what focus flows.</param>
    /// <param name="flows">Collection of flows to other children.</param>
    /// <param name="focusLose">Keys indicating that current <see cref="FocusFlowManager"/> must lose focus.</param>
    internal ChildSpecification(IFocusable from, IReadOnlyDictionary<ConsoleKey, IFocusable> flows, ImmutableList<ConsoleKey> focusLose)
    {
        From = from;
        Flows = flows;
        FocusLose = focusLose;
    }
}