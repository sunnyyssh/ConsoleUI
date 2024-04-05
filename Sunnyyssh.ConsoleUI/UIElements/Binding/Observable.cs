using System.Runtime.CompilerServices;

namespace Sunnyyssh.ConsoleUI;

public sealed class Observable<TValue> 
    : IObservable<TValue, UpdatedEventArgs>
{
    private TValue? _value;

    public TValue? Value
    {
        get => _value;
        set
        {
            _value = value;
            OnUpdated();
        }
    }
    
    public event UpdatedEventHandler<TValue, UpdatedEventArgs>? Updated;

    private void OnUpdated([CallerMemberName] string? propertyName = null)
    {
        var args = new UpdatedEventArgs(propertyName!);
        
        Updated?.Invoke(this, args);
    }

    public Observable()
    { }

    public Observable(TValue initialValue)
    {
        _value = initialValue;
    }
}