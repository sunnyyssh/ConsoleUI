// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI.Binding;

public class ValueChangedEventArgs<TValue> : UpdatedEventArgs
{
    public TValue NewValue { get; }

    public ValueChangedEventArgs(TValue newValue)
    {
        NewValue = newValue;
    }
}