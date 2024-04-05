namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when application initialization goes wrong.
/// </summary>
public class ApplicationInitException : ConsoleUIException
{
    public ApplicationInitException(string? message) : base(message)
    { }
}