namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when focus flow process goes wrong.
/// </summary>
public class FocusFlowException : ConsoleUIException
{
    public FocusFlowException(string? message) : base(message)
    {
    }
}