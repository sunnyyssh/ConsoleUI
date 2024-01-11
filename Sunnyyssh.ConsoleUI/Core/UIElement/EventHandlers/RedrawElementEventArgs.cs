namespace Sunnyyssh.ConsoleUI;

internal sealed class RedrawElementEventArgs
{
    public RedrawOptions RedrawOptions { get; private init; }

    public RedrawElementEventArgs(RedrawOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        RedrawOptions = options;
    }
}