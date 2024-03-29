namespace Sunnyyssh.ConsoleUI;

internal static partial class CollectionExtensions
{
    public static OptionElementsCollection<T> ToCollection<T>(this IEnumerable<T> elements)
        where T : OptionElement
    {
        var elementsArr = elements.ToArray();

        return new OptionElementsCollection<T>(elementsArr);
    }
}