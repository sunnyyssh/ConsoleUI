namespace Sunnyyssh.ConsoleUI.Exceptions;

public class TooManyOptionsException : ConsoleUIException
{
    public TooManyOptionsException(string message) : base(message)
    {
        
    }
    
    public TooManyOptionsException() : base("Too many options was added.")
    {
        
    }
}