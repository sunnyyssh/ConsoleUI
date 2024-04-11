using System.Collections.Immutable;
using System.Runtime.InteropServices.ComTypes;
using Sunnyyssh.ConsoleUI;
using Sunnyyssh.ConsoleUI.Binding;

namespace Test;

public static class Test6
{
    public static Application Initialize()
    {
        var settings = new ApplicationSettings()
        {
            Width = 30,
            Height = 30,
            KillApplicationKey = ConsoleKey.Escape
        };
        var appBuilder = new ApplicationBuilder(settings);

        var headers = new[] { "Id", "Name", "Cost" }.ToImmutableList();

        var rows = GridRowDefinition.From(1, 2, 2, 2);
        var columns = GridColumnDefinition.From(1, 10, 7);

        var gridDefinition = new GridDefinition(rows, columns);
        
        var tableBuilder1 = new ViewTableBuilder(Size.FullSize, headers, gridDefinition, 10)
        {
            BorderLineKind = LineKind.Double,
            BorderColorNotFocused = Color.Blue,
            BorderColorFocused = Color.DarkBlue,
        };
        
        var tableBuilder2 = new ViewTableBuilder(Size.FullSize, headers, gridDefinition, 10)
        {
            BorderColorNotFocused = Color.Red,
        };

        var grid = new GridBuilder(Size.FullSize, new GridDefinition(GridRowDefinition.From(1, 1), GridColumnDefinition.From(1)))
            {
                FocusUpKeys = ImmutableList.Create(ConsoleKey.W),
                FocusDownKeys = ImmutableList.Create(ConsoleKey.S),
            }
            .Add(tableBuilder1, 0, 0)
            .Add(tableBuilder2, 1, 0);
        
        var app = appBuilder
            .Add(grid, Position.LeftTop)
            .Build();

        Subscribe(app);

        return app;
    }

    private static void Subscribe(Application app)
    {
        var grid = (Grid)app.Children.Single().Child;

        var table1 = (ViewTable)grid.Children[0].Child;

        var table2 = (ViewTable)grid.Children[1].Child;

        var initTable = new BindableDataTable<string?>(table1.DataRowsCount, table1.HeadersCount, null);
        
        for (int i = 0; i < initTable.RowCount; i++)
        {
            initTable[i, 0] = i.ToString();
            
            initTable[i, 2] = DateTime.Now.Ticks.ToString();
        }
        
        void TableChange(DataTableUpdateArgs<string?> args, ref BindableDataTable<string?> changing) => 
            changing[args.Row, args.Column] = args.NewValue;

        var invertible =
            new InvertibleBindable<BindableDataTable<string?>, DataTableUpdateArgs<string?>>(initTable, TableChange);
        
        table1.Bind(invertible.FirstSide);
        table2.Bind(invertible.SecondSide);
    }
}