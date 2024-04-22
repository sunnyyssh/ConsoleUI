// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when UI process or initialization goes wrong.
/// </summary>
public abstract class ConsoleUIException : Exception
{
    protected ConsoleUIException(string? message) : base(message)
    { }

    protected ConsoleUIException()
    { }
}