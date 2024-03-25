namespace Sunnyyssh.ConsoleUI;

public interface IUIElementBuilder
{
    Size Size { get; }

    UIElement Build(UIElementBuildArgs args);
}

public interface IUIElementBuilder<out TElement> : IUIElementBuilder
    where TElement : UIElement
{
    new TElement Build(UIElementBuildArgs args);
}