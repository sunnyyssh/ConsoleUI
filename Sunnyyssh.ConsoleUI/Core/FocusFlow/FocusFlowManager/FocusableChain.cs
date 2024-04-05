using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Immutable collection of <see cref="IFocusable"/> children. Helps flow focus.
/// </summary>
internal sealed class FocusableChain : IEnumerable<IFocusable>
{
    private readonly bool _loopFocusFlow;
    
    private readonly List<IFocusable> _items;

    private int _currentIndex = 0;

    /// <summary>
    /// Index of child that is a start of focus flow cycle.
    /// </summary>
    private int _cycleRootIndex = 0;

    /// <summary>
    /// Returns current child if there are any ones.
    /// </summary>
    public IFocusable? Current => _items.Any() ? _items[_currentIndex] : null;

    /// <summary>
    /// Returns child that is focused at this moment. And null is there are no focused ones.
    /// </summary>
    public IFocusable? FocusedItem { get; private set; }

    [MemberNotNullWhen(false, nameof(Current))]
    public bool IsEmpty => !_items.Any();

    /// <summary>
    /// Whether there are any <see cref="IFocusable"/> that are waiting for focus.
    /// </summary>
    public bool HasWaiting => _items.Any(f => f.IsWaitingFocus);

    /// <summary>
    /// Sets <see cref="FocusedItem"/> to null. (It means there are no focused ones.)
    /// </summary>
    /// <returns></returns>
    public bool LoseFocus()
    {
        if (FocusedItem is null)
        {
            return false;
        }

        FocusedItem = null;
        return true;
    }

    /// <summary>
    /// Sets <see cref="FocusedItem"/> to <see cref="Current"/>.
    /// </summary>
    /// <returns></returns>
    public bool SetFocusToCurrent()
    {
        FocusedItem = Current;
        return FocusedItem is null;
    }

    /// <summary>
    /// Moves to the next child that is waiting for focus.
    /// </summary>
    /// <returns>True if successfully moved. False if there are no waiting for focus.</returns>
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

    /// <summary>
    /// Just moves next.
    /// </summary>
    /// <returns>False if focus cycle root is reached.</returns>
    public bool MoveNext()
    {
        if (!_items.Any())
        {
            return false;
        }

        _currentIndex = (_currentIndex + 1) % _items.Count;

        return _currentIndex == _cycleRootIndex && !_loopFocusFlow;
    }

    /// <summary>
    /// Tries to set <see cref="Current"/> to <see cref="sender"/>.
    /// </summary>
    /// <param name="sender"><see cref="IFocusable"/> child that should be current.</param>
    /// <returns>True is successfully set. False otherwise.</returns>
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

    /// <summary>
    /// Creates <see cref="FocusableChain"/> instance.
    /// </summary>
    /// <param name="loopFocusFlow">Indicates if focus flow should be looped.</param>
    /// <param name="items">Children.</param>
    public FocusableChain(bool loopFocusFlow, IEnumerable<IFocusable> items)
    {
        _loopFocusFlow = loopFocusFlow;

        _items = new List<IFocusable>(items);
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