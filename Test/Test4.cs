using Sunnyyssh.ConsoleUI;

namespace Test;

public static class Test4
{
    public static Application Initialize()
    {
        var appBuilder = new ApplicationBuilder(new ApplicationSettings());

        var gridDefinition = new GridDefinition(
            GridRowDefinition.From(1, 1, 1, 1, 0.4, 0.5),
            GridColumnDefinition.From(1, 2, 3, 4, 5, 6));

        var gridBuilder = new GridBuilder(Size.FullSize, gridDefinition);
        
        appBuilder.Add(gridBuilder, Position.LeftTop);

        return appBuilder.Build();
    }
}