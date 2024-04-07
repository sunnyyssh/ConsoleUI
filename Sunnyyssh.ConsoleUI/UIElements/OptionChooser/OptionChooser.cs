namespace Sunnyyssh.ConsoleUI;

public class OptionChosenOnEventArgs
{
    public int OptionIndex { get; }

    public OptionElement OptionElement { get; }

    public OptionChosenOnEventArgs(int optionIndex, OptionElement optionElement)
    {
        OptionIndex = optionIndex;
        OptionElement = optionElement;
    }
}

public delegate void OptionChosenOnEventHandler(OptionChooser sender, OptionChosenOnEventArgs args);

public class OptionChosenOffEventArgs
{
    public int OptionIndex { get; }

    public OptionElement OptionElement { get; }

    public OptionChosenOffEventArgs(int optionIndex, OptionElement optionElement)
    {
        OptionIndex = optionIndex;
        OptionElement = optionElement;
    }
}

public delegate void OptionChosenOffEventHandler(OptionChooser sender, OptionChosenOffEventArgs args);

public record OptionChooserKeySet(ConsoleKey[] MoveNextKeys, ConsoleKey[] MovePreviousKeys, ConsoleKey[] ChosenOnKeys, ConsoleKey[] ChosenOffKeys)
{
    public bool IsMoveNext(ConsoleKey key) => MoveNextKeys.Contains(key);
    
    public bool IsMovePrevious(ConsoleKey key) => MovePreviousKeys.Contains(key);
    
    public bool IsChosenOn(ConsoleKey key) => ChosenOnKeys.Contains(key);
    
    public bool IsChosenOff(ConsoleKey key) => ChosenOffKeys.Contains(key);
}

public abstract class OptionChooser : UIElement, IFocusable
{
    private readonly Handler<OptionChooser, OptionChosenOnEventArgs> _chosenOnEventHandler = new(5);
    
    private readonly Handler<OptionChooser, OptionChosenOffEventArgs> _chosenOffEventHandler = new(5);
    
    private readonly OptionChooserKeySet _keySet;

    private int _currentIndex = 0;
    
    protected IReadOnlyList<OptionElement> OrderedOptions { get; }
    public bool CanChooseOnlyOne { get; }

    // ReSharper disable once NotAccessedField.Local
    private ForceTakeFocusHandler? _forceTakeFocusHandler;

    // ReSharper disable once NotAccessedField.Local
    private ForceLoseFocusHandler? _forceLoseFocusHandler;

    public bool IsWaitingFocus { get; set; } = true;

    bool IFocusable.IsWaitingFocus => IsWaitingFocus;

    public bool IsFocused { get; private set; }

    protected OptionElement Current => OrderedOptions[_currentIndex];

    protected int CurrentIndex => _currentIndex;

    public bool HandlePressedKey(ConsoleKeyInfo keyInfo)
    {
        var key = keyInfo.Key;

        if (_keySet.IsChosenOn(key) && !Current.IsChosen)
        {
            ChosenOnCurrent(out bool loseFocus);
            return !loseFocus;
        }
        
        if (_keySet.IsChosenOff(key) && Current.IsChosen)
        {
            ChosenOffCurrent(out bool loseFocus);
            return !loseFocus;
        }

        if (_keySet.IsMoveNext(key))
        {
            MoveNext();
            return true;
        }

        if (_keySet.IsMovePrevious(key))
        {
            MovePrevious();
            return true;
        }

        return true;
    }

    private void ChosenOffCurrent(out bool loseFocus)
    {
        if (!Current.IsChosen)
        {
            loseFocus = false;
            return;
        }
        
        Current.ChosenOff();
        loseFocus = false;
    }

    private void ChosenOnCurrent(out bool loseFocus)
    {
        if (Current.IsChosen)
        {
            loseFocus = false;
            return;
        }
        
        if (CanChooseOnlyOne)
        {
            foreach (var option in OrderedOptions)
            {
                if (!option.IsChosen)
                    continue;
                
                if (option == Current)
                    continue;
                
                option.ChosenOff();
            }
        }

        Current.ChosenOn();
        
        loseFocus = CanChooseOnlyOne;
        
    }

    private bool MovePrevious() => MoveOn(-1);

    private bool MoveNext() => MoveOn(1);

    private bool MoveOn(int offset)
    {
        if (_currentIndex + offset < 0 || _currentIndex + offset >= OrderedOptions.Count)
            return false;

        var pastOption = OrderedOptions[_currentIndex];
        _currentIndex = (_currentIndex + offset) % OrderedOptions.Count;
        var newOption = OrderedOptions[_currentIndex];

        if (IsFocused)
        {
            pastOption.FocusOff();
            newOption.FocusOn();
        }

        return true;
    }

    void IFocusable.TakeFocus()
    {
        IsFocused = true;
        Current.FocusOn();
    }

    void IFocusable.LoseFocus()
    {
        Current.FocusOff();
        IsFocused = false;
    }

    public event OptionChosenOnEventHandler ChosenOn
    {
        add
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _chosenOnEventHandler.Add(
                new Action<OptionChooser, OptionChosenOnEventArgs>(value), 
                true);
        }
        remove
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _chosenOnEventHandler.Remove(
                new Action<OptionChooser, OptionChosenOnEventArgs>(value));
        }
    }

    public event OptionChosenOffEventHandler ChosenOff
    {
        add
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _chosenOffEventHandler.Add(
                new Action<OptionChooser, OptionChosenOffEventArgs>(value), 
                true);
        }
        remove
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            
            _chosenOffEventHandler.Remove(
                new Action<OptionChooser, OptionChosenOffEventArgs>(value));
        }
    }
    
    event ForceTakeFocusHandler? IFocusable.ForceTakeFocus
    {
        add => _forceTakeFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceTakeFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }

    public event ForceLoseFocusHandler? ForceLoseFocus
    {
        add => _forceLoseFocusHandler += value ?? throw new ArgumentNullException(nameof(value));
        remove => _forceLoseFocusHandler -= value ?? throw new ArgumentNullException(nameof(value));
    }

    protected OptionChooser(int width, int height, IReadOnlyList<OptionElement> options, 
        OptionChooserKeySet keySet, bool canChooseOnlyOne, OverlappingPriority priority) 
        : base(width, height, priority)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(keySet, nameof(keySet));
        
        _keySet = keySet;
        OrderedOptions = options;
        CanChooseOnlyOne = canChooseOnlyOne;
    }
}