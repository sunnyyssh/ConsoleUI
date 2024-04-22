// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when listening keybord buttons goes wrong.
/// </summary>
public class KeyListeningException : ConsoleUIException
{
    public KeyListeningException(string? message) : base(message)
    { }
}