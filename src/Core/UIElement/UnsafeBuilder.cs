// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

internal class UnsafeBuilder : IUIElementBuilder
{
    private readonly IUIElementBuilder _builder;
    public Size Size { get; }
        
    public UIElement Build(UIElementBuildArgs args)
    {
        return _builder.Build(args);
    }

    public UnsafeBuilder(IUIElementBuilder builder, Size newSize)
    {
        _builder = builder;
        Size = newSize;
    }
}

internal class UnsafeBuilder<T> : IUIElementBuilder<T>
    where T : UIElement
{
    private readonly IUIElementBuilder<T> _builder;
    public Size Size { get; }
        
    public T Build(UIElementBuildArgs args)
    {
        return _builder.Build(args);
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    public UnsafeBuilder(IUIElementBuilder<T> builder, Size newSize)
    {
        _builder = builder;
        Size = newSize;
    }
}