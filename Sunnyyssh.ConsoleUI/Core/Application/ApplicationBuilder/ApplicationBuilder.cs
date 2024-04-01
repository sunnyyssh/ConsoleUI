namespace Sunnyyssh.ConsoleUI;

public sealed class ApplicationBuilder
{
    private readonly ApplicationSettings _settings;

    private readonly List<QueuedPositionChild> _orderedQueuedChildren = new();

    private readonly Dictionary<IUIElementBuilder, Dictionary<ConsoleKey, IUIElementBuilder>> _focusFlow = new();

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

        var focusFlowSpecification = InitializeFocusSpecification(orderedChildren);

        var resultApp = InitializeApplication(orderedChildren, focusFlowSpecification);

        return resultApp;
    }

    private FocusFlowSpecification InitializeFocusSpecification(ChildrenCollection orderedChildren)
    {
        var specBuilder = new FocusFlowSpecificationBuilder(true);
        
        var focusables = orderedChildren
            .Where(child => child.Child is IFocusable)
            .Select(child => (IFocusable)child.Child)
            .ToArray();

        if (focusables.Length <= 1)
        {
            if (focusables.Length == 1)
            {
                specBuilder.Add(focusables[0]);
            }
            
            return specBuilder.Build();
        }

        foreach (var focusable in focusables)
        {
            specBuilder.Add(focusable);
        }
        
        for (int i = 0; i < focusables.Length - 1; i++)
        {
            specBuilder.AddFlow(focusables[i], focusables[i + 1], _settings.FocusChangeKeys);
        }

        specBuilder.AddFlow(focusables[^1], focusables[0], _settings.FocusChangeKeys);

        return specBuilder.Build();
    }

    private Application InitializeApplication(ChildrenCollection orderedChildren, FocusFlowSpecification focusFlowSpecification)
    {
        // Now no additional implementations of Application are needed.
        return new DefaultApplication(_settings, orderedChildren, focusFlowSpecification);
    }
    
    public ApplicationBuilder(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        
        _settings = settings;
    }
}