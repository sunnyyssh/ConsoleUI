namespace Sunnyyssh.ConsoleUI.Binding;

public interface IBindable<out TValue, TArgs> : IObservable<TValue, TArgs>
    where TArgs : UpdatedEventArgs
{
    void HandleUpdate(TArgs args);

    event UpdatedEventHandler<TValue, TArgs> BoundUpdated;

    public IObservable<TValue, TArgs> CreateBoundObservable()
    {
        return new BoundObservable<TValue, TArgs>(this);
    }
}

internal class BoundObservable<TValue, TArgs> : IObservable<TValue, TArgs>
    where TArgs : UpdatedEventArgs
{
    private readonly IBindable<TValue, TArgs> _from;
        
    public TValue Value => _from.Value;
        
    public event UpdatedEventHandler<TValue, TArgs>? Updated;

    private void OnUpdated(IObservable<TValue, TArgs> sender, TArgs args)
    {
        Updated?.Invoke(_from, args);
    }

    public BoundObservable(IBindable<TValue, TArgs> from)
    {
        _from = from;
        from.Updated += OnUpdated;
    }
}