using Sunnyyssh.ConsoleUI;
using Sunnyyssh.ConsoleUI.Binding;

namespace Test;

public static class Test3
{
    public static Application Initialize()
    {
        var appSettings = new ApplicationSettings();
        var appBuilder = new ApplicationBuilder(appSettings);

        var textBoxBuilder = new TextBoxBuilder(Size.FullSize);

        var definition = new GridDefinition(GridRowDefinition.From(1, 1), GridColumnDefinition.From(1));

        var gridBuilder = new GridBuilder(Size.FullSize, definition) { BorderKind = BorderKind.SingleLine }
            .Add(textBoxBuilder, 0, 0)
            .Add(textBoxBuilder, 0, 1);
        
        appBuilder.Add(gridBuilder, Position.LeftTop);

        var app = appBuilder.Build();

        var grid = (Grid)app.Children.Single().Child;

        var bindable = new BindableObject<string?>(null);

        var textBoxes = grid.Children.Select(ch => (TextBox)ch.Child).ToArray();
        
        textBoxes[0].Bind(bindable);

        bindable.BoundUpdated += (_, args) => textBoxes[1].Text = args.NewValue;

        return app;
    }
}