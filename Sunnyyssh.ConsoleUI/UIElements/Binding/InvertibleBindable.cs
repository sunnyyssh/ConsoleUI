// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI.Binding;

public delegate void BoundDataChanger<TData, in TArgs>(TArgs args, ref TData changing)
    where TArgs : UpdatedEventArgs;

public sealed class InvertibleBindable<TData, TArgs>
    where TArgs : UpdatedEventArgs
{
    public IBindable<TData, TArgs> FirstSide { get; }

    public IBindable<TData, TArgs> SecondSide { get;  }
    
    public InvertibleBindable(TData initState, BoundDataChanger<TData, TArgs> changer)
    {
        var first = new UnsafeBindable(initState, changer);
        var second = new UnsafeBindable(initState, changer);

        first.BoundUpdated += second.Update;
        second.BoundUpdated += first.Update;

        FirstSide = first;
        SecondSide = second;
    }
    
    private sealed class UnsafeBindable : IBindable<TData, TArgs>
    {
        private readonly BoundDataChanger<TData, TArgs> _changer;
        
        private TData _value;

        public TData Value => _value;

        public void Update(IObservable<TData, TArgs> from, TArgs args)
        {
            _changer(args, ref _value);
            Updated?.Invoke(this, args);
        }
        
        public event UpdatedEventHandler<TData, TArgs>? Updated;

        public void HandleUpdate(TArgs args)
        {
            _changer(args, ref _value);
            BoundUpdated?.Invoke(this, args);
        }

        public event UpdatedEventHandler<TData, TArgs>? BoundUpdated;

        public UnsafeBindable(TData initState, BoundDataChanger<TData,TArgs> changer)
        {
            ArgumentNullException.ThrowIfNull(changer, nameof(changer));
            
            _changer = changer;
            _value = initState;
        }
    }
}