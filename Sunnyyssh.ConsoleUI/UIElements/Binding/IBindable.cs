namespace Sunnyyssh.ConsoleUI;

public interface IBindable<out TValue, TArgs> : IObservable<TValue, TArgs>
    where TArgs : UpdatedEventArgs
{
    void HandleUpdate(TArgs args);

    event UpdatedEventHandler<TValue, TArgs> BoundUpdate;
}