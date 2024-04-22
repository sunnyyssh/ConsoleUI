// Developed by Bulat Bagaviev (@sunnyyssh).
// This file is licensed to you under the MIT license.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Args of building <see cref="UIElement"/> instance.
/// </summary>
public class UIElementBuildArgs
{
    /// <summary>
    /// The absolute width of building <see cref="UIElement"/> instance.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// The absolute height of building <see cref="UIElement"/> instance.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Creates an instance of <see cref="UIElementBuildArgs"/>.
    /// </summary>
    /// <param name="width">
    /// The absolute width of building <see cref="UIElement"/> instance.
    /// </param>
    /// <param name="height">
    /// The absolute height of building &lt;see cref="UIElement"/&gt; instance.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public UIElementBuildArgs(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), width, null);
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height), height, null);
        
        Width = width;
        Height = height;
    }
}