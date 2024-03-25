using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class ApplicationBuilder
{
    private readonly ApplicationSettings _settings;

    private readonly List<QueuedChild> _orderedQueuedChildren = new();

    public ApplicationBuilder Add(UIElement element, Position position)
    {
        _orderedQueuedChildren.Add(new QueuedChild(element, position));

        return this;
    }
    
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
}