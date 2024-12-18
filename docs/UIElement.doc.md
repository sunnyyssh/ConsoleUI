<h1>UIElement</h1>

UIElemnt is base class for all elements in UI which may be drawn. It's the only simple role of this class.
You can watch at its source code in [UIElement.cs](../../src/Core/UIElement/UIElement.cs)

Useful:
- [Custom UIElement implementation](CustomUIElement.doc.md)
- [Its role in Core](Core.doc.md)

Every UIElement is created with fixed absolute size so relational sizes must be resolved in [IUIElementBuilder](../../src/Core/UIElement/IUIElementBuilder.cs). 
That's why every UIElement must have its own builder.
It causes some problems. For example, if you add builder it's problematically to get built instance directly:
```csharp
ApplicationBuilder appBuilder = ... // Here appBuilder is initialized.
IUIElementBuilder elementBuilder = ... // Here elementBuilder is initialized.
var app = appBuilder.Add(elementBuilder, Position.LeftTop)
    .Build();
// And how to get created element now?
// I think you should understand the problem.
// You may suppose that every builder must create only one instance and then we'll be able to get instance by its builder.
// But I want one builder to have an ability to create many elements.
```
The solution is below:
```csharp
ApplicationBuilder appBuilder = ... // Here appBuilder is initialized.
IUIElementBuilder elementBuilder = ... // Here elementBuilder is initialized.
var app = appBuilder.Add(elementBuilder, Position.LeftTop, out BuiltUIElement builtElement)
    .Build();

builtElement.WaitInitialization();
UIElement element = builtElement.Element;
```

Why actually I decided to make all elements be created with fixed absolute size?
<br/>
The one of the reasons is that if I add element to the Wrapper and Wrapper's size is not resolved then it's impossible to resolve relational element's size.
That's why I decided to resolve size in Build() method of the builder. It resolves sizes sequentially from the root to the added elemets.

UIElement gives opportunity to inheritors to redraw it by Redraw method:
<br/>
If hideOverlap is true then previous state is hidden by this state. But if this state doesn't have some pixels then previous's pixels will be current. 

```csharp
protected void Redraw(DrawState state, bool hideOverlap = true)
```
Also it saves its current state. It can be useful:

```csharp
protected internal DrawState? CurrentState { get; private set; }
```
CurentState is null if state is not created. You can check if it is created by IsStateInitialized property:

```csharp
public bool IsStateInitialized { get; private set; }
```

Also there are some cases when you should know whether you element is drawn now:

```csharp
public bool IsDrawn { get; private set; }
```

Also if some elements are overlapped (intersected) and wrapper or any other their owner allows overlapping then their owner must resolved which one lies higher.
<br/>
For this reason, each element has OverlappingPriority parameter.
```csharp
public OverlappingPriority Priority { get; }
```

```csharp
public enum OverlappingPriority
{
    Lowest,
    Low,
    Medium,
    High,
    Highest
}
```