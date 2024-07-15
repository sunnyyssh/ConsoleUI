// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when drawing process goes wrong.
/// </summary>
public class DrawingException : ConsoleUIException
{
    public DrawingException(string? message) : base(message)
    { }
}