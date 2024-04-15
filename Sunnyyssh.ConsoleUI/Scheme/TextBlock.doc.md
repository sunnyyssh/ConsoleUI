<h1>TextBlock</h1>

```csharp
public sealed class TextBlock : UIElement
```
You can find its source code in <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/UIElements/TextBlock/TextBlock.cs">TextBlock.cs</a>

<h2>Building</h2>
To build TextBlock you should use TextBlockBuilder. (Its source code is in <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/UIElements/TextBlock/TextBlockBuilder.cs">TextBlockBuilder.cs</a>)
<br/>

```csharp
public sealed class TextBlockBuilder : IUIElementBuilder<TextBlock>
```

Here is an example:

```csharp
using Sunnyyssh.ConsoleUI;

var appBuilder = new ApplicationBuilder(
    new ApplicationSettings() { DefaultForeground = Color.Gray }); // app builder init.

var textBlockBuilder = new TextBlockBuilder(30, 10)
{
    Background = Color.Transparent, // It will have a background of ubnderlying.
    Foreground = Color.Default, // It will have a default foreground color (sepcified by ApplicationSettings).
    WordWrap = true, // Words should be wrapped.
    TextHorizontalAligning = HorizontalAligning.Center,
    TextVerticalAligning = VerticalAligning.Center
};

var app = appBuilder
    .Add(textBlockBuilder, Position.LeftTop, out var builtTextBlock) // Add textBlockBuilder at left top position.
    .Build(); // Application builds.

builtTextBlock.WaitInitialization(); // Waits till it's initialized. (Actually, it won't wait because it's built with application).
var textBlock = builtTextBlock.Element; // Gettings built TextBlock instance.
// Every second TextBlock's text will be updated to the current time.
using var timer = new Timer(_ => textBlock.Text = $"{DateTime.Now:T}", null, 0, 1000);

app.Run();
// It's waiting because timer will be disposed otherwise. (Just delete to check).
app.Wait();
```

It runs to this:
<br/>
<img src="TextBlock.demo.gif">

