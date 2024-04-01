using System.Text;

namespace Sunnyyssh.ConsoleUI;

internal static class TextHelper
{
    private static readonly char[] SplitChars = new char[] { ' ', '-', '\t' };
    
    public static string[] SplitText(int width, bool wordWrap, string text)
    {
        if (!wordWrap)
        {
            return text
                .Chunk(width)
                .Select(chars => new string(chars))
                .ToArray();
        }

        return WordsWrap(text, width).ToArray(); 
    }

    public static void PlaceText(int left, int top, int width, int height, 
        bool wordWrap, string text, Color background, Color foreground,
        VerticalAligning textVerticalAligning, HorizontalAligning textHorizontalAligning, DrawStateBuilder builder)
    {
        var lines = TextHelper.SplitText(width, wordWrap, text);

        int startingTop = top + (lines.Length >= height || textVerticalAligning == VerticalAligning.Top 
            ? 0
            : textVerticalAligning == VerticalAligning.Center 
                ? (height - lines.Length) / 2
                : height - lines.Length);
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (startingTop + i >= top + height)
                break;

            string line = lines[i].Length > width ? lines[i][..width] : lines[i];
            int lineLeft = left + textHorizontalAligning switch
            {
                HorizontalAligning.Left => 0,
                HorizontalAligning.Center => (width - line.Length) / 2,
                HorizontalAligning.Right => width - line.Length,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            builder.Place(lineLeft, startingTop + i, line, background, foreground);
        }
    }

    
    private static IEnumerable<string> WordsWrap(string str, int width) // TODO code your own algorythm.
    {
        string[] words = Explode(str, SplitChars);

        int curLineLength = 0;
        var strBuilder = new StringBuilder();
        for(int i = 0; i < words.Length; i += 1)
        {
            string word = words[i];
            // If adding the new word to the current line would be too long,
            // then put it on a new line (and split it up if it's too long).
            if (curLineLength + word.Length > width)
            {
                // Only move down to a new line if we have text on the current line.
                // Avoids situation where
                // wrapped whitespace causes emptylines in text.
                if (curLineLength > 0)
                {
                    yield return strBuilder.ToString();
                    strBuilder.Clear();
                    curLineLength = 0;
                }

                // If the current word is too long
                // to fit on a line (even on its own),
                // then split the word up.
                while (word.Length > width)
                {
                    strBuilder.Append(word.Substring(0, width - 1) + "-");
                    word = word.Substring(width - 1);

                    yield return strBuilder.ToString();
                    strBuilder.Clear();
                }

                // Remove leading whitespace from the word,
                // so the new line starts flush to the left.
                word = word.TrimStart();
            }
            strBuilder.Append(word);
            curLineLength += word.Length;
        }

        if (strBuilder.Length != 0)
        {
            yield return strBuilder.ToString();
        }
    }

    private static string[] Explode(string str, char[] splitChars)
    {
        List<string> parts = new List<string>();
        int startIndex = 0;
        while (true)
        {
            int index = str.IndexOfAny(splitChars, startIndex);
            
            if (index == -1)
            {
                parts.Add(str.Substring(startIndex));
                return parts.ToArray();
            }

            string word = str.Substring(startIndex, index - startIndex);
            char nextChar = str.Substring(index, 1)[0];
            // Dashes and the like should stick to the word occuring before it.
            // Whitespace doesn't have to.
            if (char.IsWhiteSpace(nextChar))
            {
                parts.Add(word);
                parts.Add(nextChar.ToString());
            }
            else
            {
                parts.Add(word + nextChar);
            }

            startIndex = index + 1;
        }
    }
}