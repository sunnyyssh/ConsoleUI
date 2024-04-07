namespace Sunnyyssh.ConsoleUI;

public class ValueChangedEventArgs<TValue> : UpdatedEventArgs
{
    public TValue NewValue { get; }

    public ValueChangedEventArgs(TValue newValue)
    {
        NewValue = newValue;
    }
}