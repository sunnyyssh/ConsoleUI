using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

internal class QueuedChild
{
    public IUIElementBuilder? Builder { get; } = null;

    public UIElement? Element { get; } = null;

    [MemberNotNullWhen(true, nameof(Element))]
    [MemberNotNullWhen(false, nameof(Builder))]
    public bool IsInstance { get; }

    public QueuedChild(IUIElementBuilder builder)
    {
        Builder = builder;
        IsInstance = false;
    }
    
    public QueuedChild(UIElement element)
    {
        Element = element;
        IsInstance = true;
    }
}

internal class QueuedPositionChild : QueuedChild
{
    public Position Position { get; }
    
    public QueuedPositionChild(IUIElementBuilder builder, Position position) : base(builder)
    {
        Position = position;
    }
        
    public QueuedPositionChild(UIElement element, Position position) : base(element)
    {
        Position = position;
    }
}