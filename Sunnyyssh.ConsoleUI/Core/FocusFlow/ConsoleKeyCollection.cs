using System.Collections;

namespace Sunnyyssh.ConsoleUI;

public class ConsoleKeyCollection : IReadOnlyList<ConsoleKey>
{
    public static readonly ConsoleKeyCollection Empty = new ConsoleKeyCollection(Array.Empty<ConsoleKey>());
    
    private readonly IReadOnlyList<ConsoleKey> _keys;

    public IEnumerator<ConsoleKey> GetEnumerator() => _keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_keys).GetEnumerator();

    public int Count => _keys.Count;

    public ConsoleKey this[int index] => _keys[index];

    public static ConsoleKeyCollection From(IEnumerable<ConsoleKey> keys)
    {
        var keysArr = keys.ToArray();

        return new ConsoleKeyCollection(keysArr);
    }
    
    private ConsoleKeyCollection(ConsoleKey[] keys)
    {
        _keys = keys;
    }
}

public static partial class CollectionExtensions
{
    public static ConsoleKeyCollection ToCollection(this IEnumerable<ConsoleKey> keys)
    {
        return ConsoleKeyCollection.From(keys);
    }
}