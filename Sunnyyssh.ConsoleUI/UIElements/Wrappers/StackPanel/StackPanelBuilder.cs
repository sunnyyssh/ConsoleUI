namespace Sunnyyssh.ConsoleUI;

public sealed class StackPanelBuilder : IUIElementBuilder<StackPanel>
{
    private readonly List<QueuedChildWithOffset> _orderedQueuedChildren = new();
    
    public Size Size { get; }

    public ConsoleKey[] FocusChangeKeys { get; init; } = new [] { ConsoleKey.Tab };
    
    public Orientation Orientation { get; }

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public StackPanelBuilder Add(IUIElementBuilder builder, int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset));
        
        _orderedQueuedChildren.Add(new QueuedChildWithOffset(builder, offset));

        return this;
    }

    public StackPanel Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        
        int width = args.Width;
        int height = args.Height;

        var buildedChildren = Orientation switch
        {
            Orientation.Vertical => 
                BuildVertical(width, height),
            
            Orientation.Horizontal => 
                BuildHorizontal(width, height),
            
            _ => throw new ArgumentOutOfRangeException()
        };

        var resultStackPanel = new StackPanel(width, height, buildedChildren, 
            Orientation, FocusChangeKeys, OverlappingPriority);

        return resultStackPanel;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    private ChildInfo[] BuildHorizontal(int width, int height)
    {
        var placer = new ElementsFieldBuilder(width, height, 
            // false because StackPanel should contain children sequentially.
            false);

        int accumulatedLeft = 0;
        int absoluteTop = 0;

        foreach (var queuedChild in _orderedQueuedChildren)
        {
            accumulatedLeft += queuedChild.Offset;
            var position = new Position(accumulatedLeft, absoluteTop);
            
            ChildInfo childInfo;
            
            placer.Place(queuedChild.Builder, position, out childInfo);

            accumulatedLeft += childInfo.Width;
        }

        return placer.Build();
    }

    private ChildInfo[] BuildVertical(int width, int height)
    {
        var placer = new ElementsFieldBuilder(width, height, 
            // false because StackPanel should contain children sequentially.
            false);
        
        int absoluteLeft = 0;
        int accumulatedTop = 0;

        foreach (var queuedChild in _orderedQueuedChildren)
        {
            accumulatedTop += queuedChild.Offset;
            var position = new Position(absoluteLeft, accumulatedTop);
            
            ChildInfo childInfo;
            
            placer.Place(queuedChild.Builder, position, out childInfo);
            
            accumulatedTop += childInfo.Height;
        }

        return placer.Build();
    }

    public StackPanelBuilder(Size size, Orientation orientation)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));
        
        Size = size;
        Orientation = orientation;
    }
    
    private class QueuedChildWithOffset : QueuedChild
    {
        public int Offset { get; }

        public QueuedChildWithOffset(IUIElementBuilder builder, int offset) : base(builder)
        {
            Offset = offset;
        }
    }
}