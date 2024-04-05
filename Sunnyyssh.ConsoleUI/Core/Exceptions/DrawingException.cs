namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when drawing process goes wrong.
/// </summary>
public class DrawingException : ConsoleUIException
{
    public DrawingException(string? message) : base(message)
    { }
}