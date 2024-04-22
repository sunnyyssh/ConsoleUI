// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

internal sealed class CharSetBorder : Border
{
    internal CharSetBorder(int width, int height, BorderCharSet charSet, Color color, OverlappingPriority priority) 
        : base(width, height, charSet, color, priority)
    { }
}
