// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

using System.Diagnostics.Contracts;
using System.Text;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Helps work with characters.
/// </summary>
internal static class CharHelper
{
    private static readonly char[] SpecialChars = { '\n', '\t', '\r', '\b', '\f', '\v', '\a' };

    /// <summary>
    /// Removes special characters from string or replaces it by neutral ones.
    /// </summary>
    /// <param name="text"><see cref="string"/> instance to remove special characters from.</param>
    /// <param name="ignore">True if special characters should be simply removed. False if it's needed to attempt to replace them by their meaning.</param>
    /// <returns>Transfromed string.</returns>
    [Pure]
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
                // Removing '\b'
                builder.Remove(pos, 1);
                continue;
            }

            // Removing '\b' and previous characters.
            builder.Remove(pos - 1, 2);
            pos--;
        }

        return builder.ToString();
    }

    /// <summary>
    /// Indicates if given character is special.
    /// </summary>
    /// <param name="c">Character to check.</param>
    /// <returns>True if special. False otherwise.</returns>
    public static bool IsCharSpecial(char c)
    {
        for (int i = 0; i < SpecialChars.Length; i++)
            if (SpecialChars[i] == c)
                return true;
        return false;
    }
}