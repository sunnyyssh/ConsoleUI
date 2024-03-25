using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

internal readonly struct QueuedChild
{
    public IUIElementBuilder? Builder { get; } = null;

    public UIElement? Element { get; } = null;

    public Position Position { get; }
        
    [MemberNotNullWhen(true, nameof(Element))]
    [MemberNotNullWhen(false, nameof(Builder))]
    public bool IsInstance { get; }
        
    public QueuedChild(IUIElementBuilder builder, Position position)
    {
        Builder = builder;
        IsInstance = false;
        Position = position;
    }
        
    public QueuedChild(UIElement element, Position position)
    {
        Element = element;
        IsInstance = true;
        Position = position;
    }
}