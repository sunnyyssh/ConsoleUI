namespace Sunnyyssh.ConsoleUI;

public sealed class ApplicationBuilder
{
    private readonly ApplicationSettings _settings;

    private readonly List<QueuedPositionChild> _orderedQueuedChildren = new();

    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, int left, int top)
        => Add(elementBuilder, new Position(left, top));
    
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, int left, double topRelation)
        => Add(elementBuilder, new Position(left, topRelation));
    
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, int top)
        => Add(elementBuilder, new Position(leftRelation, top));
    
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, double topRelation)
        => Add(elementBuilder, new Position(leftRelation, topRelation));
    
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, Position position)
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, position));
        
        return this;
    }

    public Application Build()
    {
        int initWidth = _settings.Width ?? Drawer.WindowWidth;
        int initHeight = _settings.Height ?? Drawer.WindowHeight;
        
        var placementBuilder = new ElementsFieldBuilder(initWidth, initHeight, _settings.EnableOverlapping);
        
        foreach (var queuedChild in _orderedQueuedChildren)
        {
            placementBuilder.Place(queuedChild.Builder, queuedChild.Position);
        }

        var orderedChildren = placementBuilder.Build();

        var resultApp = InitializeApplication(orderedChildren);

        return resultApp;
    }

    private Application InitializeApplication(ChildrenCollection orderedChildren)
    {
        // Now no additional implementations of Application are needed.
        return new DefaultApplication(_settings, orderedChildren);
    }
    
    public ApplicationBuilder(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        
        _settings = settings;
    }
}