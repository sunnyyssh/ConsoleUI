// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

public sealed class UIElementInitializer<TUIElement> : IUIElementInitializer<TUIElement>
    where TUIElement : UIElement
{
    private readonly ManualResetEvent _waitEvent = new(false);
    
    public TUIElement? Element { get; private set; }
    
    UIElement? IUIElementInitializer.Element => Element;

    public bool IsInitialized => Element is not null;

    public void WaitInitialization()
    {
        _waitEvent.WaitOne();
    }

    public void Initialize(TUIElement element)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        
        Element = element;
        _waitEvent.Set();
    }

    void IUIElementInitializer.Initialize(UIElement element) => Initialize((TUIElement)element);
}