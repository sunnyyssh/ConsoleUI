using Sunnyyssh.ConsoleUI;

namespace Test;

public static class Test7
{
    public static Application Initialize()
    {
        var settings = new ApplicationSettings();
        var appBuilder = new ApplicationBuilder(settings);

        var rectangleBuilder = new RectangleBuilder(Size.FullSize, Color.Green);
        
        var gridBuilder = new GridBuilder(Size.FullSize,
            new GridDefinition(GridRowDefinition.From(1, 1, 1), GridColumnDefinition.From(1, 1, 1)))
            .Add(rectangleBuilder, 0, 0, out var rectangle1)
            .Add(rectangleBuilder, 1, 2, out var rectangle2);

        var canvasBuilder = new CanvasBuilder(Size.FullSize)
            .Add(rectangleBuilder, Position.LeftTop, out var rectangle3)
            .Add(rectangleBuilder, Position.LeftTop, out var rectangle4);
        
        var stackPanelBuilder = new StackPanelBuilder(Size.FullSize, Orientation.Vertical)
            .Add(rectangleBuilder, out var rectangle5);

        var textOptionBuilder = new TextOptionElementBuilder(Size.FullSize, "chupapi munana",
            new TextOptionColorSet(Color.Blue, Color.DarkBlue));

        var rowChooserBuilder = new RowChooserBuilder(Size.FullSize, Orientation.Vertical)
            .Add(textOptionBuilder, out var textOption);
        
        var switcherBuilder = new UIElementSwitcherBuilder(Size.FullSize)
            .Add(gridBuilder, out var grid)
            .Add(canvasBuilder, out var canvas)
            .Add(stackPanelBuilder, out var stackPanel)
            .Add(rowChooserBuilder, out var chooser);

        appBuilder.Add(switcherBuilder, Position.LeftTop, out var switcher);

        var app = appBuilder.Build();

        return app;
    }
}