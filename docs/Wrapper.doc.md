<h1>Wrapper</h1>

Wrapper is a base class of all elements that contain other elements. It has its own [FocusFlowManager](../src/Core/FocusFlow/FocusFlowManager/FocusFlowManager.cs) (Read about it in [Core docs](Core.doc.md)).

```csharp
public abstract class Wrapper : UIElement, IFocusManagerHolder
```
You can find its source code in <a href="../src/Core/Wrapper/Wrapper.cs">Wrapper.cs</a>

<h3>Inheritors</h3>
1. [CompositionWrapper](Wrapper.doc.md) class in <a href="../src/Core/Wrapper/CompositionWrapper.cs">CompositionWrapper.cs</a>
2. [Canvas](Canvas.doc.md) class in [Canvas.cs](../src/UIElements/Wrappers/Canvas/Canvas.cs)
3. [Grid](Grid.doc.md) class in [Grid.cs](../src/UIElements/Wrappers/Grid/Grid.cs)
4. [StackPanel](StackPanel.doc.md) class in [StackPanel.cs](../src/UIElements/Wrappers/StackPanel/StackPanel.cs)

You can implement your own wrapper. Maybe Wrapper's implementations in the source code will help you. It gives many opportuninties, so here we go.

<h1>CompositionWrapper</h1>

CompositionWrapper is a Wrapper that creates its state with composition of its children states. 

```csharp
public abstract class CompositionWrapper : Wrapper
```
You can find its source code in [CompositionWrapper.cs](../src/Core/Wrapper/CompositionWrapper.cs)

<h3>Inheritors</h3>
1. [Canvas](Canvas.doc.md) class in [Canvas.cs](../src/UIElements/Wrappers/Canvas/Canvas.cs)
2. [Grid](Grid.doc.md) class in [Grid.cs](../src/UIElements/Wrappers/Grid/Grid.cs)
3. [StackPanel](StackPanel.doc.md) class in [StackPanel.cs](../src/UIElements/Wrappers/StackPanel/StackPanel.cs)

You can implement your own CompositionWrapper. Maybe CompositionWrapper's implementations in the source code will help you. It gives many opportuninties, so here we go.


