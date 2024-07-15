<h1>How to use it?</h1>

Here is the simple explanation how to use this library.
<br/>
The usage of each element is in its own documentation. In many cases it will help you. [Here you can find links to element's doc.](https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/README.md#ui-elements)

You will have a better understanding if you understand [how Core works](Core.doc.md).
<br/>
But here is the brief guid how to build your Application instance and run it.

Every element has its own builder. The reason of this colution is partly explained [here](UIElement.doc.md)
<br/>
So, first off You should initialize ApplicationBuilder with specific settings:

```csharp
using System.Collections.Immutable;
using Sunnyyssh.ConsoleUI;

var appSettings = new ApplicationSettings
{
    DefaultForeground = Color.Black, // Default foreground color is Black.
    DefaultBackground = Color.White, // Defaulr foreground color is White.
    FocusChangeKeys = ImmutableList.Create(ConsoleKey.Tab), // Tab will change focus from one IFocusable to another one. (Tab also is set by default so you shouldn't specify it every time).
    EnableOverlapping = true, // Children can overlap each other.
    KillApplicationKey = ConsoleKey.Escape // Escape press will cause Application.Stop() invocation.
};
var appBuilder = new ApplicationBuilder(appSettings);
```

Then you should create some element's builders you want to place (In this example TextBox is placed in the Grid):

```csharp
// ... (code above)

// In the builder's constructor you muist specify its size.
var textBoxBuilder = new TextBoxBuilder(widthRelation: 0.5, heightRelation: 0.5);

var stackPanelBuilder = new StackPanelBuilder(Size.FullSize, Orientation.Horizontal);
// builtTextBox gives an opportunity to get built TextBox when StackPanel is built (As for it, it is built when Application is built).
stackPanelBuilder.Add(textBoxBuilder, out BuiltUIElement<TextBox> builtTextBox);
```

Then you should add your builders to appBuilder to be built to the children of Application instance.

```csharp
// ... (code above)

// We must specify the position of adding builder.
// Be careful!!! If size and position have such values that element can't be placed in the box then you will get an exception.
appBuilder.Add(stackPanelBuilder, Position.LeftTop);
```

Now we should build Application and Run it:

```csharp
// ... (code above)

var app = appBuilder.Build();

// Application will be running from this moment.
app.Run();

// In many cases we should wait while the applcation is running.
// For example, it is used not to allow some objects be finilized by GC
// Or most common case: We should do something after Applcation has stopped.
app.Wait();
```