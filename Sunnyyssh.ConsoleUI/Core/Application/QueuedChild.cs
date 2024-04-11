namespace Sunnyyssh.ConsoleUI;

internal class QueuedChild
{
    public IUIElementBuilder Builder { get; }
    
    public IUIElementInitializer? Initializer { get; }

    public QueuedChild(IUIElementBuilder builder, IUIElementInitializer? initializer)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        Builder = builder;
        Initializer = initializer;
    }
}

internal class QueuedPositionChild : QueuedChild
{
    public Position Position { get; }
    
    public QueuedPositionChild(IUIElementBuilder builder, IUIElementInitializer? initializer, Position position) 
        : base(builder, initializer)
    {
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        Position = position;
    }
}