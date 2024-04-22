// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI.Binding;

public sealed class ObservableObject<TValue> : IObservable<TValue, ValueChangedEventArgs<TValue>>
{
    private TValue _value;

    public TValue Value
    {
        get => _value;
        set
        {
            _value = value;
            Updated?.Invoke(this, new ValueChangedEventArgs<TValue>(_value));
        }
    }

    public event UpdatedEventHandler<TValue, ValueChangedEventArgs<TValue>>? Updated;

    public ObservableObject(TValue initValue)
    {
        _value = initValue;
    }
}