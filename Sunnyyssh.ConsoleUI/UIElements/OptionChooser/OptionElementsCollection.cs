using System.Collections;

namespace Sunnyyssh.ConsoleUI;

public sealed class OptionElementsCollection<T> : IReadOnlyList<T>
    where T : OptionElement
{
    private readonly IReadOnlyList<T> _elements;

    public IEnumerator<T> GetEnumerator() => _elements.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_elements).GetEnumerator();

    public int Count => _elements.Count;

    public T this[int index] => _elements[index];

    internal OptionElementsCollection(T[] elements)
    {
        _elements = elements;
    }
}