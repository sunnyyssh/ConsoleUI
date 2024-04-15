using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public sealed class UIElementSwitcherBuilder : IUIElementBuilder<UIElementSwitcher>
{
    public Size Size { get; }

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public bool OverridesFlow { get; init; } = false;
    
    private readonly List<QueuedChild> _queuedChildren = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder">Be careful! Its own size is ignored and it will be full-size.</param>
    /// <returns></returns>
    public UIElementSwitcherBuilder Add(IUIElementBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        
        _queuedChildren.Add(new QueuedChild(builder, null));

        return this;
    }
    
    public UIElementSwitcherBuilder Add(IUIElementBuilder builder, 
        out BuiltUIElement builtUIElement)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        
        var initializer = new UIElementInitializer<UIElement>();
        builtUIElement = new BuiltUIElement(initializer);

        _queuedChildren.Add(new QueuedChild(builder, initializer));

        return this;
    }
    
    public UIElementSwitcherBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> builder, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        
        var initializer = new UIElementInitializer<TUIElement>();
        builtUIElement = new BuiltUIElement<TUIElement>(initializer);

        _queuedChildren.Add(new QueuedChild(builder, initializer));

        return this;
    }
    
    public UIElementSwitcher Build(UIElementBuildArgs args)
    {
        int width = args.Width;
        int height = args.Height;

        var canvasStates = InitializeCanvasStates(width, height);

        var excludingSpecification = InitializeFocusSpecification(canvasStates);
        
        var resultSwitcher = new UIElementSwitcher(width, height, canvasStates, excludingSpecification, OverlappingPriority);

        return resultSwitcher;
    }

    private FocusFlowSpecification InitializeFocusSpecification(ImmutableList<Canvas> canvasStates)
    {
        var specBuilder = new FocusFlowSpecificationBuilder(true);

        foreach (var canvasState in canvasStates)
        {
            specBuilder.Add(canvasState);
        }

        return specBuilder.Build();
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    private ImmutableList<Canvas> InitializeCanvasStates(int width, int height)
    {
        return _queuedChildren
            .Select(child => BuildChildToCanvas(width, height, child))
            .ToImmutableList();
    }

    private Canvas BuildChildToCanvas(int width, int height, QueuedChild child)
    {
        var canvasBuilder = new CanvasBuilder(width, height)
        {
            FocusChangeKeys = ImmutableList<ConsoleKey>.Empty,
            OverridesFocusFlow = OverridesFlow,
            EnableOverlapping = false,
            FocusFlowLoop = false
        };

        var fullSizeBuilder = child.Builder.UnsafeWithSize(Size.FullSize);

        var buildArgs = new UIElementBuildArgs(width, height);

        if (child.Initializer is null)
        {
            return canvasBuilder
                .Add(fullSizeBuilder, Position.LeftTop)
                .Build(buildArgs);
        }

        var result = canvasBuilder
            .Add(fullSizeBuilder, Position.LeftTop, out var builtUIElement)
            .Build(buildArgs);

        if (!builtUIElement.IsInitialized)
            throw new InvalidOperationException();
        
        child.Initializer.Initialize(builtUIElement.Element);

        return result;
    }

    public UIElementSwitcherBuilder(int width, int height)
        : this(new Size(width, height))
    { }

    public UIElementSwitcherBuilder(int width, double heightRelational)
        : this(new Size(width, heightRelational))
    { }

    public UIElementSwitcherBuilder(double widthRelational, int height)
        : this(new Size(widthRelational, height))
    { }

    public UIElementSwitcherBuilder(double widthRelational, double heightRelational)
        : this(new Size(widthRelational, heightRelational))
    { }
    
    public UIElementSwitcherBuilder(Size size)
    {
        Size = size;
    }
}