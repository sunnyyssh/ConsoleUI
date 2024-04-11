using Sunnyyssh.ConsoleUI;

namespace Test;

public static class Test5
{
    public static Application Initialize()
    {
        var settings = new ApplicationSettings()
        {
            KillApplicationKey = ConsoleKey.Escape,
        };
        var appBuilder = new ApplicationBuilder(settings);

        var textBox1 = new TextBoxBuilder(0.7, 1.0)
        {
            FocusedBorderColor = Color.Yellow,
        };
        var button1 = new ButtonBuilder(0.3, 1.0)
        {
            FocusedBorderColor = Color.Red,
            PressedBackground = Color.Magenta,
            ShowPress = true,
        };
        var stack1 = new StackPanelBuilder(Size.FullSize, Orientation.Horizontal)
            .Add(textBox1)
            .Add(button1);

        var chooser2 = new RowTextChooserBuilder(Size.FullSize, Orientation.Vertical, 
             new[] { "пропадает в миллионах на век", "когда-то самый дорогой человек" });
        
        var gridDefinition = new GridDefinition(
            GridRowDefinition.From(1, 1, 1, 1, 0.4, 0.5),
            GridColumnDefinition.From(1, 2, 3, 4, 5, 6));

        var grid2 = new GridBuilder(Size.FullSize, gridDefinition)
            .Add(chooser2, 5, 2);

        var switcher = new UIElementSwitcherBuilder(Size.FullSize)
            .Add(stack1)
            .Add(grid2);

        var app = appBuilder
            .Add(switcher, Position.LeftTop)
            .Build();

        Subscribe(app);

        return app;
    }

    private static void Subscribe(Application app)
    {
        var switcher = (UIElementSwitcher)app.Children.Single().Child;

        var firstState = switcher.PresentationStates[0];

        var stack1 = (StackPanel)firstState.Children.Single().Child;

        var button1 = (Button)stack1.Children.Single(ch => ch.Child is Button).Child;

        button1.UnsafeRegisterPressedHandler((_, _) => switcher.SwitchTo(1));
    }
}