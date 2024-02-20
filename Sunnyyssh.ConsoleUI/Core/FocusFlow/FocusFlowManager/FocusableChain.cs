using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

// This type is not thread-safe. But it must be used in thread-safe context.
internal sealed class FocusableChain : IEnumerable<IFocusable>
{
    private readonly bool _loopFocusFlow;
    
    private readonly List<IFocusable> _items = new();

    private int _currentIndex = 0;

    private int _cycleRootIndex = 0;

    public IFocusable? Current => _items.Any() ? _items[_currentIndex] : null;

    public IFocusable? FocusedItem { get; private set; }

    [MemberNotNullWhen(false, nameof(Current))]
    public bool IsEmpty => !_items.Any();

    public bool HasWaiting => _items.Any(f => f.IsWaitingFocus);

    public bool LoseFocus()
    {
        if (FocusedItem is null)
        {
            return false;
        }

        FocusedItem = null;
        return true;
    }

    public bool SetFocusToCurrent()
    {
        FocusedItem = Current;
        return FocusedItem is null;
    }

    public bool MoveNextWaitingFocus()
    {
        if (!_items.Any(f => f.IsWaitingFocus))
        {
            return false;
        }

        for (int i = 0; i < _items.Count; i++)
        {
            _currentIndex = (_currentIndex + 1) % _items.Count;

            if (_currentIndex == _cycleRootIndex && !_loopFocusFlow)
            {
                break;
            }
            
            if (_items[_currentIndex].IsWaitingFocus)
            {
                return true;
            }
        }

        return false;
    }

    public bool MoveNext()
    {
        if (!_items.Any())
        {
            return false;
        }

        _currentIndex = (_currentIndex + 1) % _items.Count;

        return _currentIndex == _cycleRootIndex && !_loopFocusFlow;
    }

    public bool TrySetCurrentTo(IFocusable sender)
    {
        int index = _items.IndexOf(sender);
        if (index < 0)
        {
            return false;
        }

        _currentIndex = index;
        return true;
    }

    #region Items adding or removing
    
    public bool TryRemove(IFocusable focusable)
    {
        int indexOfRemoving = _items.IndexOf(focusable);
        if (indexOfRemoving < 0)
        {
            return false;
        }

        if (_currentIndex > indexOfRemoving)
        {
            _currentIndex--;
        }

        if (_cycleRootIndex > indexOfRemoving)
        {
            _cycleRootIndex--;
        }

        _items.RemoveAt(indexOfRemoving);

        _currentIndex %= _items.Count;
        _cycleRootIndex %= _items.Count;

        return true;
    }

    public void InsertAt(int index, IFocusable focusable)
    {
        _items.Insert(index, focusable);
        
        if (_currentIndex >= index)
        {
            _currentIndex++;
        }

        if (_cycleRootIndex >= index)
        {
            _cycleRootIndex++;
        }
    }

    public bool TryInsertAfter(IFocusable afterThis, IFocusable newFocusable)
    {
        int index = _items.IndexOf(afterThis);
        if (index < 0)
        {
            return false;
        }
        
        InsertAt(index + 1, newFocusable);

        return true;
    }

    public bool TryInsertBefore(IFocusable beforeThis, IFocusable newFocusable)
    {
        int index = _items.IndexOf(beforeThis);
        if (index < 0)
        {
            return false;
        }
        
        InsertAt(index, newFocusable);

        return true;
    }

    public void Add(IFocusable focusable)
    {
        if (!_items.Any())
        {
            _currentIndex = 0;
        }
        _items.Add(focusable);
    }

    #endregion

    public FocusableChain(bool loopFocusFlow)
    {
        _loopFocusFlow = loopFocusFlow;
    }

    public IEnumerator<IFocusable> GetEnumerator()
    {
        return ((IEnumerable<IFocusable>)_items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }
}