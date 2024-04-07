using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public sealed class UIElementSwitcherBuilder : IUIElementBuilder<UIElementSwitcher>
{
    public Size Size { get; }

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;
    
    private readonly List<QueuedChild> _queuedChildren = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder">Be careful! Its own size is ignored and it will be full-size.</param>
    /// <returns></returns>
    public UIElementSwitcherBuilder Add(IUIElementBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        
        _queuedChildren.Add(new QueuedChild(builder));

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
            .Select(ch => InitializeCanvas(width, height, ch.Builder))
            .ToImmutableList();
    }

    private Canvas InitializeCanvas(int width, int height, IUIElementBuilder builder)
    {
        var canvasBuilder = new CanvasBuilder(width, height)
        {
            FocusChangeKeys = ImmutableList<ConsoleKey>.Empty,
            OverridesFocusFlow = true,
            EnableOverlapping = false,
            FocusFlowLoop = false
        };

        var fullSizeBuilder = builder.UnsafeWithSize(Size.FullSize);

        var buildArgs = new UIElementBuildArgs(width, height);

        return canvasBuilder
            .Add(fullSizeBuilder, Position.LeftTop)
            .Build(buildArgs);
    }

    public UIElementSwitcherBuilder(Size size)
    {
        Size = size;
    }
}