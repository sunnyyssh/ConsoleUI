using System.Text;

namespace Sunnyyssh.ConsoleUI;

internal static class CharHelper
{
    private static readonly char[] SpecialChars = { '\n', '\t', '\r', '\b', '\f', '\v', '\a' };

    public static string? RemoveSpecialChars(string? text, bool ignore)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        var builder = new StringBuilder(text);
        
        if (ignore)
        {
            builder.Replace("\b", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace("\v", "")
                .Replace("\r", "")
                .Replace("\f", "")
                .Replace("\a", "");
            
            return builder.ToString();
        }

        builder.Replace('\n', ' ')
            .Replace('\t', ' ')
            .Replace('\v', ' ')
            .Replace("\r", "")
            .Replace("\f", "")
            .Replace("\a", "");

        int pos = 1;
        while (pos < builder.Length)
        {
            if (builder[pos] != '\b')
            {
                pos++;
                continue;
            }

            if (pos == 0)
            {
                builder.Remove(pos, 1);
                continue;
            }

            builder.Remove(pos - 1, 2);
            pos--;
        }

        return builder.ToString();
    }

    public static bool IsCharSpecial(char c)
    {
        for (int i = 0; i < SpecialChars.Length; i++)
            if (SpecialChars[i] == c)
                return true;
        return false;
    }
}