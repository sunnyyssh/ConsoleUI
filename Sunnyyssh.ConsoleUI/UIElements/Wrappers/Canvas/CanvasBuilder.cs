// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

public sealed class CanvasBuilder : IUIElementBuilder<Canvas>
{
    private readonly List<QueuedPositionChild> _orderedQueuedChildren = new();
    
    public Size Size { get; }

    public bool FocusFlowLoop { get; init; } = false;

    public bool OverridesFocusFlow { get; init; } = true;
    
    public bool EnableOverlapping { get; init; } = true;

    public ImmutableList<ConsoleKey> FocusChangeKeys { get; init; } = new[] { ConsoleKey.Tab }.ToImmutableList();

    public CanvasBuilder Add(IUIElementBuilder elementBuilder, int left, int top)
        => Add(elementBuilder, new Position(left, top));
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, int left, double topRelation)
        => Add(elementBuilder, new Position(left, topRelation));
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, int top)
        => Add(elementBuilder, new Position(leftRelation, top));
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, double topRelation)
        => Add(elementBuilder, new Position(leftRelation, topRelation));
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, Position position)
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, null, position));
        
        return this;
    }

    public CanvasBuilder Add(IUIElementBuilder elementBuilder, int left, int top, out BuiltUIElement builtUIElement)
        => Add(elementBuilder, new Position(left, top), out builtUIElement);
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, int left, double topRelation, out BuiltUIElement builtUIElement)
        => Add(elementBuilder, new Position(left, topRelation), out builtUIElement);
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, int top, out BuiltUIElement builtUIElement)
        => Add(elementBuilder, new Position(leftRelation, top), out builtUIElement);
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, double topRelation, out BuiltUIElement builtUIElement)
        => Add(elementBuilder, new Position(leftRelation, topRelation), out builtUIElement);
    
    public CanvasBuilder Add(IUIElementBuilder elementBuilder, Position position, out BuiltUIElement builtUIElement)
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        var initializer = new UIElementInitializer<UIElement>();
        builtUIElement = new BuiltUIElement(initializer);
        
        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, initializer, position));
        
        return this;
    }

    public CanvasBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, int left, int top, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(left, top), out builtUIElement);
    
    public CanvasBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, int left, double topRelation, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(left, topRelation), out builtUIElement);
    
    public CanvasBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, double leftRelation, int top, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(leftRelation, top), out builtUIElement);
    
    public CanvasBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, 
        double leftRelation, double topRelation, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(leftRelation, topRelation), out builtUIElement);
    
    public CanvasBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, Position position, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        var initializer = new UIElementInitializer<TUIElement>();
        builtUIElement = new BuiltUIElement<TUIElement>(initializer);

        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, initializer, position));
        
        return this;
    }
        
    public Canvas Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        var placementBuilder = new ElementsFieldBuilder(args.Width, args.Height, EnableOverlapping);
        
        foreach (var queuedChild in _orderedQueuedChildren)
        {
            placementBuilder.Place(queuedChild.Builder, queuedChild.Position, out var childInfo);

            if (queuedChild.Initializer is not null)
            {
                queuedChild.Initializer.Initialize(childInfo.Child);
            }
        }

        var orderedChildren = placementBuilder.Build();

        var focusFlowSpecification = InitializeFocusSpecification(orderedChildren);
        
        var resultCanvas = new Canvas(args.Width, args.Height, focusFlowSpecification, orderedChildren);

        return resultCanvas;
    }

    private FocusFlowSpecification InitializeFocusSpecification(IReadOnlyList<ChildInfo> orderedChildren)
    {
        var specBuilder = new FocusFlowSpecificationBuilder(OverridesFocusFlow);
        
        var focusables = orderedChildren
            .Where(child => child.IsFocusable)
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
            specBuilder.AddFlow(focusables[i], focusables[i + 1], FocusChangeKeys);
        }

        if (FocusFlowLoop)
        {
            specBuilder.AddFlow(focusables[^1], focusables[0], FocusChangeKeys);
        }
        else
        {
            specBuilder.AddLoseFocus(focusables[^1], FocusChangeKeys);
        }

        return specBuilder.Build();
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public CanvasBuilder(int width, int height)
        : this(new Size(width, height))
    { }
    
    public CanvasBuilder(int width, double heightRelation)
        : this(new Size(width, heightRelation))
    { }
    
    public CanvasBuilder(double widthRelation, int height)
        : this(new Size(widthRelation, height))
    { }
    
    public CanvasBuilder(double widthRelation, double heightRelation)
        : this(new Size(widthRelation, heightRelation))
    { }
    
    public CanvasBuilder(Size size)
    {
        ArgumentNullException.ThrowIfNull(size, nameof(size));

        Size = size;
    }
}