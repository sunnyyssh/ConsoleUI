namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Exception is thrown when listening keybord buttons goes wrong.
/// </summary>
public class KeyListeningException : ConsoleUIException
{
    public KeyListeningException(string? message) : base(message)
    {
    }
}