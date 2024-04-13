namespace Sunnyyssh.ConsoleUI;

internal sealed class Handler<TArg>
{
    private readonly int? _maxCostMilliseconds;
    
    private readonly List<PrimitiveHandler> _handlers = new();
    
    public void Invoke(TArg arg)
    {
        ArgumentNullException.ThrowIfNull(arg, nameof(arg));
        
        if (_maxCostMilliseconds is null)
        {
            InvokeNaive(arg);
            return;
        }

        InvokeWithCost(arg, _maxCostMilliseconds.Value);
    }

    private void InvokeNaive(TArg arg)
    {
        foreach (var (handler, toInvokeParallel) in _handlers)
        {
            if (toInvokeParallel)
            {
                Task.Run(() => handler(arg));
                continue;
            }

            handler(arg);
        }
    }

    private void InvokeWithCost(TArg arg, int maxMilliseconds)
    {
        using var counter = new TimeCounter(maxMilliseconds);

        foreach (var (handler, toInvokeParallel) in _handlers)
        {
            // If user was wrong about cost of each "cheap" operation
            // handler should invoke others in parallel mode.
            if (counter.HasReachedMax || toInvokeParallel)
            {
                Task.Run(() => handler(arg));
                continue;
            }

            handler(arg);
        }
    }

    public void Add(Action<TArg> handler, bool isExpensive)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        
        _handlers.Add(new PrimitiveHandler(handler, isExpensive));
    }

    public bool Remove(Action<TArg> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        
        return 0 != _handlers.RemoveAll(primHandler => primHandler.Handler == handler);
    }

    public Handler()
    {
        _maxCostMilliseconds = null;
    }

    public Handler(int maxCostMilliseconds)
    {
        if (maxCostMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(maxCostMilliseconds), maxCostMilliseconds, null);
        
        _maxCostMilliseconds = maxCostMilliseconds;
    }
    
    private readonly struct PrimitiveHandler
    {
        public readonly Action<TArg> Handler;

        public readonly bool ToInvokeParallel;

        public void Deconstruct(out Action<TArg> handler, out bool toInvokeParallel)
        {
            handler = Handler;
            toInvokeParallel = ToInvokeParallel;
        }

        public PrimitiveHandler(Action<TArg> handler, bool toInvokeParallel)
        {
            ToInvokeParallel = toInvokeParallel;
            Handler = handler;
        }
    }
}


internal sealed class Handler<TArg1, TArg2>
{
    private readonly int? _maxCostMilliseconds;
    
    private readonly List<PrimitiveHandler> _handlers = new();
    
    public void Invoke(TArg1 arg1, TArg2 arg2)
    {
        ArgumentNullException.ThrowIfNull(arg1, nameof(arg1));
        ArgumentNullException.ThrowIfNull(arg2, nameof(arg2));
        
        if (_maxCostMilliseconds is null)
        {
            InvokeNaive(arg1, arg2);
            return;
        }

        InvokeWithCost(arg1, arg2, _maxCostMilliseconds.Value);
    }

    private void InvokeNaive(TArg1 arg1, TArg2 arg2)
    {
        foreach (var (handler, toInvokeParallel) in _handlers)
        {
            if (toInvokeParallel)
            {
                Task.Run(() => handler(arg1, arg2));
                continue;
            }

            handler(arg1, arg2);
        }
    }

    private void InvokeWithCost(TArg1 arg1, TArg2 arg2, int maxMilliseconds)
    {
        using var counter = new TimeCounter(maxMilliseconds);

        foreach (var (handler, toInvokeParallel) in _handlers)
        {
            // If user was wrong about cost of each "cheap" operation
            // handler should invoke others in parallel mode.
            if (counter.HasReachedMax || toInvokeParallel)
            {
                Task.Run(() => handler(arg1, arg2));
                continue;
            }

            handler(arg1, arg2);
        }
    }

    public void Add(Action<TArg1, TArg2> handler, bool isExpensive)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _handlers.Add(new PrimitiveHandler(handler, isExpensive));
    }

    public bool Remove(Action<TArg1, TArg2> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        
        return 0 != _handlers.RemoveAll(primHandler => primHandler.Handler == handler);
    }

    public Handler()
    {
        _maxCostMilliseconds = null;
    }

    public Handler(int maxCostMilliseconds)
    {
        _maxCostMilliseconds = maxCostMilliseconds;
    }
    
    private readonly struct PrimitiveHandler
    {
        public readonly Action<TArg1, TArg2> Handler;

        public readonly bool ToInvokeParallel;

        public void Deconstruct(out Action<TArg1, TArg2> handler, out bool toInvokeParallel)
        {
            handler = Handler;
            toInvokeParallel = ToInvokeParallel;
        }

        public PrimitiveHandler(Action<TArg1, TArg2> handler, bool toInvokeParallel)
        {
            ToInvokeParallel = toInvokeParallel;
            Handler = handler;
        }
    }
}


