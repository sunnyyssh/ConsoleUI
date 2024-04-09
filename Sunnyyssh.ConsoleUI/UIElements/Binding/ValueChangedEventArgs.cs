namespace Sunnyyssh.ConsoleUI.Binding;

public class ValueChangedEventArgs<TValue> : UpdatedEventArgs
{
    public TValue NewValue { get; }

    public ValueChangedEventArgs(TValue newValue)
    {
        NewValue = newValue;
    }
}