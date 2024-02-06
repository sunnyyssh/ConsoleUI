using System.Collections;
using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

[DebuggerStepThrough]
public sealed class ConcurrentChain<T> : IEnumerable<T>
    where T : class
{
    private readonly List<T> _items = new();

    private readonly object _lockObject = new();
    
    private int _cycleRootIndex = 0;

    private int _current = 0;

    public bool MoveNext()
    {
        lock (_lockObject)
        {
            if (!_items.Any())
                return false;
            _current = (_current + 1) % _items.Count;
            return _current != _cycleRootIndex;
        }
    }

    public T? Current
    {
        get
        {
            lock (_lockObject)
            {
                return _items.Any() ? _items[_current] : null;
            }
        }
    }

    public int Count
    {
        get
        {
            lock (_lockObject)
            {
                return _items.Count();
            }
        }
    }

    public bool IsEmpty
    {
        get
        {
            lock (_lockObject)
            {
                return !_items.Any();
            }
        }
    }

    public bool TrySetCurrentTo(T item)
    {
        lock (_lockObject)
        {
            int index = _items.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            _current = index;
            return true;
        }
    }
    
    public bool TryRemove(T item)
    {
        lock (_lockObject)
        {
            int index = _items.IndexOf(item);
            if (index < _current)
                _current--;
            if (index < _current)
                _current--;
            return _items.Remove(item);
        }
    }

    public void Add(T item)
    {
        lock (_lockObject)
        {
            _items.Add(item);
        }
    }

    public void InsertAt(int index, T item)
    {
        lock (_lockObject)
        {
            _items.Insert(index, item);
            if (index <= _current)
                _current++;
            if (index <= _cycleRootIndex)
                _cycleRootIndex++;
        }
    }

    public bool TryInsertAfter(T after, T item)
    {
        lock (_lockObject)
        {
            int index = _items.IndexOf(after);
            if (index < 0)
                return false;
            if (index < _current)
                _current++;
            if (index < _cycleRootIndex)
                _cycleRootIndex++;
            _items.Insert(index + 1, item);
        }
        return true;
    }
    
    public bool TryInsertBefore(T before, T item)
    {
        lock (_lockObject)
        {
            int index = _items.IndexOf(before);
            if (index < 0)
                return false;
            if (index <= _current)
                _current++;
            if (index <= _cycleRootIndex)
                _cycleRootIndex++;
            _items.Insert(index, item);
        }
        return true;
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }
}