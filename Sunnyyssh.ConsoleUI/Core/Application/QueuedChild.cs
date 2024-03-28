using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

internal class QueuedChild
{
    public IUIElementBuilder Builder { get; }

    public QueuedChild(IUIElementBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        Builder = builder;
    }
}

internal class QueuedPositionChild : QueuedChild
{
    public Position Position { get; }
    
    public QueuedPositionChild(IUIElementBuilder builder, Position position) : base(builder)
    {
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        Position = position;
    }
}