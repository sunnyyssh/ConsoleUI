namespace Sunnyyssh.ConsoleUI;

public class ApplicationInitException : ConsoleUIException
{
    public ApplicationInitException(string? message) : base(message)
    {
    }
}