// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Specifies a priority of overlapping <see cref="UIElement"/> children when they are placed intersected.
/// </summary>
public enum OverlappingPriority
{
    Lowest,
    Low,
    Medium,
    High,
    Highest
}