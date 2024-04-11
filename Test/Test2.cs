using Sunnyyssh.ConsoleUI;

namespace Test;

public static class Test2
{
    public static Application Initialize()
    {
        var appSettings = new ApplicationSettings();
        var appBuilder = new ApplicationBuilder(appSettings);

        var chooserBuilder = new RowTextChooserBuilder(Size.FullSize, Orientation.Vertical,
            new [] { "Pu pu pu", "pa pa pa", ":-)"});

        return appBuilder.Add(chooserBuilder, Position.LeftTop)
            .Build();
    }
}