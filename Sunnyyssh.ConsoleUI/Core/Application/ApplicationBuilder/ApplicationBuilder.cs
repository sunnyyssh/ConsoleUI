using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Creates <see cref="Application"/> instance.
/// Gives opportunity to add <see cref="UIElement"/> by their builders (<see cref="IUIElementBuilder{TElement}"/>)
/// and specialize additional settings.
/// Cannot be inherited.
/// </summary>
/// <example>
/// <code>
/// var settings = new ApplicationSettings()
/// {
///    KillApplicationKey = ConsoleKey.Escape,
///    DefaultBackground = Color.DarkGray,
///    DrawingDelay = true,
/// };
///        
/// var appBuilder = new ApplicationBuilder(settings);
///
/// var textBox = new TextBoxBuilder(0.5, 0.4);
///
/// appBuilder.Add(textBox, Position.LeftTop);
/// 
/// var app = appBuilder.Build();
/// </code>
/// </example>
public sealed class ApplicationBuilder
{
    private readonly ApplicationSettings _settings;

    /// <summary>
    /// Added builders and their position are queued here till <see cref="Build"/> invocation.
    /// </summary>
    private readonly List<QueuedPositionChild> _orderedQueuedChildren = new();
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="left">Left absolute position (counted in characters).</param>
    /// <param name="top">Top absolute position (counted in characters).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, int left, int top)
        => Add(elementBuilder, new Position(left, top));
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="left">Left absolute position (counted in characters).</param>
    /// <param name="topRelation">Top relational position. Counts from height of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, int left, double topRelation)
        => Add(elementBuilder, new Position(left, topRelation));
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="leftRelation">Left relational position. Counts from width of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="top">Top absolute position (counted in characters).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, int top)
        => Add(elementBuilder, new Position(leftRelation, top));
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="leftRelation">Left relational position. Counts from width of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="topRelation">Top relational position. Counts from height of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, double topRelation)
        => Add(elementBuilder, new Position(leftRelation, topRelation));

    /// <summary>
    /// Queues <see cref="elementBuilder"/> at specified position.
    /// After <see cref="Build"/> invocation <see cref="elementBuilder"/>'s <see cref="IUIElementBuilder.Build"/> is invoked.
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="position">The position to place at.</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, Position position)
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        
        // Queues builder and its position till Build() invocation.
        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, null, position));

        return this;
    }

    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="left">Left absolute position (counted in characters).</param>
    /// <param name="top">Top absolute position (counted in characters).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, int left, int top, out BuiltUIElement builtUIElement) // TODO docs
        => Add(elementBuilder, new Position(left, top), out builtUIElement);
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="left">Left absolute position (counted in characters).</param>
    /// <param name="topRelation">Top relational position. Counts from height of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, int left, double topRelation, out BuiltUIElement builtUIElement)
        => Add(elementBuilder, new Position(left, topRelation), out builtUIElement);
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="leftRelation">Left relational position. Counts from width of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="top">Top absolute position (counted in characters).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, int top, out BuiltUIElement builtUIElement)
        => Add(elementBuilder, new Position(leftRelation, top), out builtUIElement);
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="leftRelation">Left relational position. Counts from width of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="topRelation">Top relational position. Counts from height of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, double leftRelation, double topRelation, out BuiltUIElement builtUIElement)
        => Add(elementBuilder, new Position(leftRelation, topRelation), out builtUIElement);

    /// <summary>
    /// Queues <see cref="elementBuilder"/> at specified position.
    /// After <see cref="Build"/> invocation <see cref="elementBuilder"/>'s <see cref="IUIElementBuilder.Build"/> is invoked.
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="position">The position to place at.</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add(IUIElementBuilder elementBuilder, Position position, out BuiltUIElement builtUIElement)
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        var initializer = new UIElementInitializer<UIElement>();
        builtUIElement = new BuiltUIElement(initializer);
        
        // Queues builder and its position till Build() invocation.
        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, initializer, position));

        return this;
    }

    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="left">Left absolute position (counted in characters).</param>
    /// <param name="top">Top absolute position (counted in characters).</param>
    /// <param name="builtUIElement"><see cref="UIElement"/> instance will be built when application is built
    /// and you can get it using this object.</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, int left, int top,
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(left, top), out builtUIElement);
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="left">Left absolute position (counted in characters).</param>
    /// <param name="topRelation">Top relational position. Counts from height of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="builtUIElement"><see cref="UIElement"/> instance will be built when application is built
    /// and you can get it using this object.</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, int left, double topRelation,
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(left, topRelation), out builtUIElement);
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="leftRelation">Left relational position. Counts from width of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="top">Top absolute position (counted in characters).</param>
    /// <param name="builtUIElement"><see cref="UIElement"/> instance will be built when application is built
    /// and you can get it using this object.</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, double leftRelation, int top,
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(leftRelation, top), out builtUIElement);
    
    /// <summary>
    /// <inheritdoc cref="Add(Sunnyyssh.ConsoleUI.IUIElementBuilder,Position)"/>
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="leftRelation">Left relational position. Counts from width of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="topRelation">Top relational position. Counts from height of placement area.
    /// (Can be more than 0 and less than or equal to 1).</param>
    /// <param name="builtUIElement"><see cref="UIElement"/> instance will be built when application is built
    /// and you can get it using this object.</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, 
        double leftRelation, double topRelation,
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
        => Add(elementBuilder, new Position(leftRelation, topRelation), out builtUIElement);

    /// <summary>
    /// Queues <see cref="elementBuilder"/> at specified position.
    /// After <see cref="Build"/> invocation <see cref="elementBuilder"/>'s <see cref="IUIElementBuilder.Build"/> is invoked.
    /// </summary>
    /// <param name="elementBuilder">The builder of child to add.</param>
    /// <param name="position">The position to place at.</param>
    /// <param name="builtUIElement"><see cref="UIElement"/> instance will be built when application is built
    /// and you can get it using this object.</param>
    /// <returns>Same instance of <see cref="ApplicationBuilder"/> to chain invocations.</returns>
    public ApplicationBuilder Add<TUIElement>(IUIElementBuilder<TUIElement> elementBuilder, Position position, 
        out BuiltUIElement<TUIElement> builtUIElement)
        where TUIElement : UIElement
    {
        ArgumentNullException.ThrowIfNull(elementBuilder, nameof(elementBuilder));
        ArgumentNullException.ThrowIfNull(position, nameof(position));

        var initializer = new UIElementInitializer<TUIElement>();
        builtUIElement = new BuiltUIElement<TUIElement>(initializer);
        
        // Queues builder, initializer and its position till Build() invocation.
        _orderedQueuedChildren.Add(new QueuedPositionChild(elementBuilder, initializer, position));

        return this;
    }

    /// <summary>
    /// Builds <see cref="Application"/> instance.
    /// </summary>
    /// <returns>Created <see cref="Application"/> instance.</returns>
    public Application Build()
    {
        int initWidth = _settings.Width ?? Drawer.WindowWidth;
        int initHeight = _settings.Height ?? Drawer.WindowHeight;
        
        var placementBuilder = new ElementsFieldBuilder(initWidth, initHeight, _settings.EnableOverlapping);
        
        foreach (var queuedChild in _orderedQueuedChildren)
        {
            placementBuilder.Place(queuedChild.Builder, queuedChild.Position, out var childInfo);
            
            queuedChild.Initializer?.Initialize(childInfo.Child);
        }

        var orderedChildren = placementBuilder.Build();

        var focusFlowSpecification = InitializeFocusSpecification(orderedChildren);

        var resultApp = InitializeApplication(orderedChildren, focusFlowSpecification);

        return resultApp;
    }

    // Initializes the specification.
    // It's just linear flow. One after one.
    private FocusFlowSpecification InitializeFocusSpecification(IReadOnlyList<ChildInfo> orderedChildren)
    {
        var specBuilder = new FocusFlowSpecificationBuilder(true);
        
        // Children that implement IFocusable and that are needed to take part in focus flow.
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

        // Cycle flow.
        specBuilder.AddFlow(focusables[^1], focusables[0], _settings.FocusChangeKeys);

        return specBuilder.Build();
    }

    // Resolves what Application instance should be created and creates it.
    private Application InitializeApplication(ImmutableList<ChildInfo> orderedChildren, FocusFlowSpecification focusFlowSpecification)
    {
        // Now no additional implementations of Application are needed.
        // In the future it may be another Application inheritor.
        return new DefaultApplication(_settings, orderedChildren, focusFlowSpecification);
    }
    
    /// <summary>
    /// Creates <see cref="ApplicationBuilder"/> instance with given settings.
    /// </summary>
    /// <param name="settings">Application settings.</param>
    public ApplicationBuilder(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        
        _settings = settings;
    }
}