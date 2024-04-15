<h1>TextBox</h1>

Element that represents box with text. It's text can be edited in UI.

```csharp
public sealed class TextBox : UIElement, IFocusable
```
You can find its source code in <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/UIElements/TextBox/TextBox.cs">TextBox.cs</a>

<h2>Building</h2>
To build TextBox you should use TextBoxBuilder. (Its source code is in <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/UIElements/TextBox/TextBoxBuilder.cs">TextBoxBuilder.cs</a>)
<br/>

```csharp
public sealed class TextBoxBuilder : IUIElementBuilder<TextBox>
```

Here is an example:

```csharp
using Sunnyyssh.ConsoleUI;

var appBuilder = new ApplicationBuilder(new ApplicationSettings()); // app builder init.

// TextBox will have 100% width and 20% height.
var urlBoxBuilder = new TextBoxBuilder(1.0, 0.2)
{
    FocusedBackground = Color.DarkBlue, // When it's focused its background is dark blue.
    StartingText = "https://google.com",
    BorderKind = BorderKind.SingleLine, 
    ShowPressedChars = true, // Pressed characters are shown.
    WordWrap = false, // Words shouldn't be wrapped.
    UserEditable = true, // Its text may be edited in UI.
    TextHorizontalAligning = HorizontalAligning.Left,
    TextVerticalAligning = VerticalAligning.Top
};

// TextBox will have 100% width and 80% height.
var responseBoxBuilder = new TextBoxBuilder(1.0, 0.8)
{
    BorderKind = BorderKind.DoubleLine,
    WordWrap = false, // Words shouldn't be wrapped.
    UserEditable = false, // Its text may not be edited in UI.
    TextHorizontalAligning = HorizontalAligning.Left,
    TextVerticalAligning = VerticalAligning.Top
};

var app = appBuilder
    .Add(urlBoxBuilder, Position.LeftTop, out var builtUrlBox) // Add textBoxBuilder at left top position.
    .Add(responseBoxBuilder, 0, 0.2, out var builtResponseBox) // Add textBoxBuilder at left top position.
    .Build(); // Application builds.
    
builtUrlBox.WaitInitialization(); // Wait when they are initialized. (Actually, they are initialized because application is built).
builtResponseBox.WaitInitialization();
var urlBox = builtUrlBox.Element; // Get built TextBox instances.
var responseBox = builtResponseBox.Element;

urlBox.TextEntered += async (_, args) => 
    // reponseBox's text is updated by response on address that is urlBox's text.
    responseBox.Text = await new HttpClient().GetStringAsync(args.Text);
    
app.Run();
app.Wait();
```

It runs to this:
<br/>
<img src="TextBox.demo.gif">

