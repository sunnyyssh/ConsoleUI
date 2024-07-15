// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public interface IUIElementInitializer
{
    public UIElement? Element { get; }

    [MemberNotNullWhen(true, nameof(Element))]
    public bool IsInitialized => Element is not null;

    [MemberNotNull(nameof(Element))]
#pragma warning disable CS8774
    public void WaitInitialization();
#pragma warning restore CS8774

    [MemberNotNull(nameof(Element))]
    public void Initialize(UIElement element);
}


public interface IUIElementInitializer<TUIElement> : IUIElementInitializer
    where TUIElement : UIElement
{
    public new TUIElement? Element { get; }

    [MemberNotNullWhen(true, nameof(Element))]
    public new bool IsInitialized { get; }

    [MemberNotNull(nameof(Element))]
    public void Initialize(TUIElement element);
}