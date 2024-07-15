// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when application initialization goes wrong.
/// </summary>
public class ApplicationInitException : ConsoleUIException
{
    public ApplicationInitException(string? message) : base(message)
    { }
}