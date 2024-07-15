// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI.Binding;

public class UpdatedEventArgs : EventArgs
{
    
}

public delegate void UpdatedEventHandler<in TValue, in TArgs>(IObservable<TValue, TArgs> sender, TArgs args)
    where TArgs : UpdatedEventArgs;

public interface IObservable<out TValue, out TArgs>
    where TArgs : UpdatedEventArgs
{
    TValue Value { get; }

    event UpdatedEventHandler<TValue, TArgs> Updated;
}