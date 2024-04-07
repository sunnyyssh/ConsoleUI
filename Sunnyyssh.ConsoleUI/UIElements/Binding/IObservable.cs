namespace Sunnyyssh.ConsoleUI;

public class UpdatedEventArgs : EventArgs
{
    
}

public delegate void UpdatedEventHandler<in TValue, in TArgs>(IObservable<TValue, TArgs> sender, TArgs args)
    where TArgs : UpdatedEventArgs;

public interface IObservable<out TValue, out TArgs>
    where TArgs : UpdatedEventArgs
{
    TValue? Value { get; }

    event UpdatedEventHandler<TValue, TArgs> Updated;
}