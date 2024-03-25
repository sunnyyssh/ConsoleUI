using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class ApplicationBuilder
{
    private readonly ApplicationSettings _settings;

    private readonly List<QueuedChild> _orderedQueuedChildren = new();
    
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, Position position)
    {
        _orderedQueuedChildren.Add(new QueuedChild(elementBuilder, position));
        
        return this;
    }

    public Application Build()
    {
        int initWidth = _settings.Width ?? Drawer.WindowWidth;
        int initHeight = _settings.Height ?? Drawer.WindowHeight;
        
        var placementBuilder = new ElementsFieldBuilder(initWidth, initHeight, _settings.EnableOverlapping);
        
        foreach (var queuedChild in _orderedQueuedChildren)
        {
            if (queuedChild.IsInstance)
            {
                placementBuilder.Place(queuedChild.Element, queuedChild.Position);
                continue;
            }

            placementBuilder.Place(queuedChild.Builder, queuedChild.Position);
        }

        var orderedChildren = placementBuilder.Build();

        var resultApp = InitializeApplication(orderedChildren);

        return resultApp;
    }

    private Application InitializeApplication(ChildInfo[] orderedChildren)
    {
        // Now no additional implementations of Application are needed.
        return new DefaultApplication(_settings, orderedChildren);
    }
    
    public ApplicationBuilder(ApplicationSettings settings)
    {
        _settings = settings;
    }
    
    private readonly struct QueuedChild
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
}