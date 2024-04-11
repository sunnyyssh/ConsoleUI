using Sunnyyssh.ConsoleUI;

namespace Test;

public static class Test1
{
    public static Application Initialize()
    {
        var settings = new ApplicationSettings()
        {
            KillApplicationKey = ConsoleKey.Escape
        };

        var appBuilder = new ApplicationBuilder(settings);
        
        var textBoxBuilder = new TextBoxBuilder(10, 4){UserEditable = false};

        var stackPanel = new StackPanelBuilder(Size.FullSize, Orientation.Horizontal) { FocusFlowLoop = false }
            .Add(textBoxBuilder);

        appBuilder.Add(stackPanel, Position.LeftTop);

        var app = appBuilder.Build();

        var singleWrapper = (Wrapper)app.Children.Single().Child;
        
        var textBox = (TextBox)singleWrapper.Children.Single(ch => ch.Child is TextBox).Child;

        foreach (var child in singleWrapper.Children)
        {
            if (child.Child is not Button button)
            {
                continue;
            }
            
            button.RegisterPressedHandler((b, args) => textBox.Text += b.Text);
        }

        return app;
    }
    
}