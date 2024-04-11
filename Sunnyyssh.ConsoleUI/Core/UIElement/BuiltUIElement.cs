using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public sealed class BuiltUIElement
{
    private readonly IUIElementInitializer _initializer;

    public UIElement? Element => _initializer.Element;

    [MemberNotNullWhen(true, nameof(Element))]
    public bool IsInitialized => Element is not null;

    [MemberNotNull(nameof(Element))]
#pragma warning disable CS8774 
    public void WaitInitialization() => _initializer.WaitInitialization();
#pragma warning restore CS8774

    public BuiltUIElement(IUIElementInitializer initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer, nameof(initializer));

        _initializer = initializer;
    }
}

public sealed class BuiltUIElement<TUIElement>
    where TUIElement : UIElement
{
    private readonly IUIElementInitializer<TUIElement> _initializer;

    public TUIElement? Element => _initializer.Element;

    [MemberNotNullWhen(true, nameof(Element))]
    public bool IsInitialized => Element is not null;

    [MemberNotNull(nameof(Element))]
#pragma warning disable CS8774 
    public void WaitInitialization() => _initializer.WaitInitialization();
#pragma warning restore CS8774

    public BuiltUIElement(IUIElementInitializer<TUIElement> initializer)
    {
        ArgumentNullException.ThrowIfNull(initializer, nameof(initializer));

        _initializer = initializer;
    }
}


