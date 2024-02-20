namespace Sunnyyssh.ConsoleUI;

public abstract class ConsoleUIException : Exception
{
    public ConsoleUIException(string? message) : base(message)
    {
        
    }

    public ConsoleUIException()
    {
        
    }
}