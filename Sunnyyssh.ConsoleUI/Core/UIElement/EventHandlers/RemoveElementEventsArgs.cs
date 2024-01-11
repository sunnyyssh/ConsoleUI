namespace Sunnyyssh.ConsoleUI;

internal sealed class RemoveElementEventsArgs
{
    public RemoveOptions RemoveOptions { get; private init; }

    public RemoveElementEventsArgs(RemoveOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        RemoveOptions = options;
    }
}