// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when it is not possible to place child (UIElement or IUIElementBuilder).
/// </summary>
public class ChildPlacementException : ConsoleUIException
{
    public ChildPlacementException(string? message) : base(message)
    { }
}