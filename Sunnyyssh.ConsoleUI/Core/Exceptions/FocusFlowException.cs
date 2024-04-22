// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when focus flow process goes wrong.
/// </summary>
public class FocusFlowException : ConsoleUIException
{
    public FocusFlowException(string? message) : base(message)
    { }
}