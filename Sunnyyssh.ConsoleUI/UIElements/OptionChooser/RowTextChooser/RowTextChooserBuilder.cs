using System.Diagnostics.Contracts;
using Sunnyyssh.ConsoleUI.Exceptions;

namespace Sunnyyssh.ConsoleUI;

public sealed class RowTextChooserBuilder : IUIElementBuilder<RowTextChooser>
{
    private static readonly OptionChooserKeySet HorizontalDefaultKeySet =
        new OptionChooserKeySet(
            new[] { ConsoleKey.RightArrow, ConsoleKey.D },
            new[] { ConsoleKey.LeftArrow, ConsoleKey.A },
            new[] { ConsoleKey.Enter },
            new[] { ConsoleKey.Enter });

    private static readonly OptionChooserKeySet VerticalDefaultKeySet =
        new OptionChooserKeySet(
            new[] { ConsoleKey.DownArrow, ConsoleKey.S },
            new[] { ConsoleKey.UpArrow, ConsoleKey.W },
            new[] { ConsoleKey.Enter },
            new[] { ConsoleKey.Enter });

    private readonly List<string> _options;

    public OptionChooserKeySet? KeySet { get; init; }

    public bool CanChooseOnlyOne { get; init; } = true;

    public Size Size { get; }

    public OverlappingPriority OverlappingPriority { get; init; } = OverlappingPriority.Medium;

    public string[] Options => _options.ToArray();

    public Orientation Orientation { get; }

    public TextOptionColorSet ColorSet { get; init; } = 
        new TextOptionColorSet(Color.Default, Color.Default);

    public void Add(string option)
    {
        ArgumentNullException.ThrowIfNull(option, nameof(option));

        _options.Add(option);
    }

    public RowTextChooser Build(UIElementBuildArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        int width = args.Width;
        int height = args.Height;

        var stackBuilder = new StackPanelBuilder(new Size(width, height), Orientation);
        
        var options = _options.ToArray();
        
        var initializedOptions = 
            stackBuilder.Orientation == Orientation.Horizontal
            ? InitializeOptionsHorizontal(width, options)
            : InitializeOptionsVertical(height, options);

        foreach (var optionBuilder in initializedOptions)
        {
            stackBuilder.Add(optionBuilder);
        }

        var initStack = stackBuilder.Build(new UIElementBuildArgs(args.Width, args.Height));

        var resultElements = initStack.Children
            // Only TextOptionElement children can be here.
            .Select(ch => (TextOptionElement)ch.Child)
            .ToCollection();

        var keySet = KeySet ?? (Orientation == Orientation.Vertical ? VerticalDefaultKeySet : HorizontalDefaultKeySet);

        var resultChooser = new RowTextChooser(width, height, 
            initStack, resultElements, keySet, 
            CanChooseOnlyOne, OverlappingPriority);

        return resultChooser;
    }

    UIElement IUIElementBuilder.Build(UIElementBuildArgs args) => Build(args);

    private IUIElementBuilder<TextOptionElement>[] InitializeOptionsVertical(int height, string[] options)
    {
        int count = options.Length;
        
        if (count > height)
            throw new TooManyOptionsException();

        double heightRelational = 1.0 / count;
        const double wideWidth = 1.0;

        var result = new IUIElementBuilder<TextOptionElement>[count];

        for (int i = 0; i < count; i++)
        {
            var textOptionBuilder = new TextOptionElementBuilder(new Size(wideWidth, heightRelational), options[i], ColorSet);

            result[i] = textOptionBuilder;
        }

        return result;
    }

    private IUIElementBuilder<TextOptionElement>[] InitializeOptionsHorizontal(int width, string[] options)
    {
        int count = options.Length;
        
        if (count > width)
            throw new TooManyOptionsException();

        double widthRelational = 1.0 / count;
        const double wideHeight = 1.0;

        var result = new IUIElementBuilder<TextOptionElement>[count];

        for (int i = 0; i < count; i++)
        {
            var textOptionBuilder = new TextOptionElementBuilder(new Size(widthRelational, wideHeight), options[i], ColorSet);

            result[i] = textOptionBuilder;
        }

        return result;
    }

    public RowTextChooserBuilder(Size size, Orientation orientation, [Pure] IEnumerable<string>? initOptions = null)
    {
        Size = size;
        Orientation = orientation;
        _options = initOptions?.ToList() ?? new List<string>();
    }
}