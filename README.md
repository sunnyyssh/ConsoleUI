<div align="center">
    <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI">
        <img src="https://readme-typing-svg.herokuapp.com?font=Fira+Code&weight=600&size=30&duration=1500&pause=4000&color=6965F7&background=98A0FF2A&center=true&random=false&width=435&lines=Sunnyyssh.ConsoleUI" alt="Typing Sunnyyssh.ConsoleUI" />
    </a>
</div>

<br/>

<a href="https://www.nuget.org/packages/Sunnyyssh.ConsoleUI/">
    <img src="https://img.shields.io/nuget/v/Sunnyyssh.ConsoleUI.svg?style=flat-square"/>
</a>

<a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI">
    <img alt="GitHub Repo stars" src="https://img.shields.io/github/stars/sunnyyssh/Sunnyyssh.ConsoleUI">
</a><br/>

<a href="https://t.me/vowtostrive">
    <img src="https://www.svgrepo.com/show/452115/telegram.svg" width="15" alt="Tg-icon"/> 
    <u><b>Check my channel</b></u>
</a>

<br/>

<a href="https://t.me/sunnyyssh">
    <img src="https://www.svgrepo.com/show/452115/telegram.svg" width="15" alt="Tg-icon"/> 
    <u><b>Write me</b></u>
</a>

<h2>Download</h2>
You can download package using NuGet:
<br/> <a href="https://www.nuget.org/packages/Sunnyyssh.ConsoleUI/">Tap link to the package.</a>

<h2>About</h2>

This library is released to give an opportunity to create UI in console using C# in .NET
<br/>

<h2>Demo</h2>

<a href="https://github.com/sunnyyssh/ToDoApp"><u>ToDoApp</u></a> - Simple to-do application in console.
<br/>
<div>
    <img height="75%" src="https://github.com/sunnyyssh/ToDoApp/raw/master/docs/light-theme-demo.gif" width="75%" alt="light-demo"/>
    <img height="75%" src="https://github.com/sunnyyssh/ToDoApp/raw/master/docs/dark-theme-demo.gif" width="75%" alt="dark-demo"/>
</div>

<h2>How to code your app</h2>
<a href="docs/Use.doc.md"><u>Detail guid here.</u></a>

<h2>How the core works</h2>
<a href="docs/Core.doc.md"><u>You can read about core in detail here.</u></a>
<br/>
A brief description: 
The application is built once with fixed size, UIElement children, focus specification and other parameters and they can't be changed.


<h2>UI elements</h2>
<a href="docs/UIElement.doc.md"><u>UIElement </u></a> class is base for all UI classes.
<br/>
<a href="docs/IFocusable.doc.md"><u>IFocusable </u></a> represents element able to handle pressed keys.
<br/>
<a href="docs/Wrapper.doc.md"><u>Wrapper </u></a> represents element containing other elements.
<br/>
And you can awlays implement your **custom UIElement** [Guid here.](docs/CustomUIElement.doc.md)

<h4>Implemented elements:</h4>


- <a href="docs/Button.doc.md"><u>Button </u></a>
<br/> Represents element able to handle presses.

- <a href="docs/TextBlock.doc.md"><u>TextBlock </u></a>
<br/> Just shows text.

- <a href="docs/TextBox.doc.md"><u>TextBox </u></a>
<br/> Shows text and can edit text in UI.

- <a href="docs/ViewTable.doc.md"><u>ViewTable </u></a> 
<br/>Table of content.

- <a href="docs/OptionChooser.doc.md"><u>RowTextChooser</u>, <u>RowOptionChooser</u>, <u>WrapperBasedChooser</u>, <u>OptionChooser </u></a> 
<br/> Represent chooser of options (like a menu). 

- <a href="docs/StackPanel.doc.md"><u>StackPanel </u></a> 
<br/>Wrapper that places children in a row (like a stack).

- <a href="docs/Grid.doc.md"><u>Grid </u></a> 
<br/>Wrapper that places children in grid cells.

- <a href="docs/Canvas.doc.md"><u>Canvas </u></a> 
<br/>Wrapper that places children at any position.

- <a href="docs/Line.doc.md"><u>Line</u>, <u>LineComposition </u></a> 
<br/>Vertical and horizontal lines.

- <a href="docs/UIElementSwitcher.doc.md"><u>UIElementSwitcher</u></a> 
<br/> Presents element that can be one of other elements and switch them.

- <a href="docs/Rectangle.doc.md"><u>Rectangle</u></a> 
<br/> The poorest rectangle in the whole world. =(

- <a href="docs/Border.doc.md"><u>Border</u></a> 
<br/> Simple border. 


UML class diagram of UI elements:
<br/>
<img alt="architecture-diagram" src="docs/UIElementsDiagram.png?raw=true"/>


