namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Builds <see cref="UIElement"/> instance.
/// </summary>
public interface IUIElementBuilder
{
    /// <summary>
    /// The size of the future <see cref="UIElement"/> instance.
    /// </summary>
    Size Size { get; }

    /// <summary>
    /// Builds <see cref="UIElement"/> instance.
    /// </summary>
    /// <param name="args">Build args.</param>
    /// <returns>Created instance.</returns>
    UIElement Build(UIElementBuildArgs args);
    
    internal IUIElementBuilder UnsafeWithSize(Size newSize)
    {
        return new UnsafeBuilder(this, newSize);
    }
}

/// <summary>
/// Builds <see cref="TUIElement"/> instance.
/// </summary>
/// <typeparam name="TUIElement">The type of <see cref="UIElement"/> inheritor.</typeparam>
public interface IUIElementBuilder<out TUIElement> : IUIElementBuilder
    where TUIElement : UIElement
{
    /// <summary>
    /// Builds <see cref="TUIElement"/> instance.
    /// </summary>
    /// <param name="args">Build args.</param>
    /// <returns>Created instance.</returns>
    new TUIElement Build(UIElementBuildArgs args);

    internal new IUIElementBuilder<TUIElement> UnsafeWithSize(Size newSize)
    {
        return new UnsafeBuilder<TUIElement>(this, newSize);
    }
}
