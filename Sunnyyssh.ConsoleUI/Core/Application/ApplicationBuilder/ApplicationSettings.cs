using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// The settings of <see cref="Application"/> instance.
/// </summary>
public sealed class ApplicationSettings
{
    private readonly Color _defaultBackground = Color.Black;
    private readonly Color _defaultForeground = Color.White;
    private readonly ImmutableList<ConsoleKey> _focusChangeKeys = new[] { ConsoleKey.Tab }.ToImmutableList();
    private readonly int? _height = null;
    private readonly int? _width = null;

    /// <summary>
    /// Specifies default background of UI. All children with <see cref="Color.Default"/> background will have this Color.
    /// </summary>
    public Color DefaultForeground
    {
        get => _defaultForeground;
        init
        {
            if (value == Color.Transparent || value == Color.Default)
            {
                return;
            }

            _defaultForeground = value;
        }
    }
    
    /// <summary>
    /// Specifies default foreground of UI. All children with <see cref="Color.Default"/> foreground will have this Color.
    /// </summary>
    public Color DefaultBackground
    {
        get => _defaultBackground;
        init
        {
            if (value == Color.Transparent || value == Color.Default)
            {
                return;
            }

            _defaultBackground = value;
        }
    }

    /// <summary>
    /// Gets or sets a Height of <see cref="Application"/> (in characters).
    /// </summary>
    /// <exception cref="NotSupportedException">Only Windows supports.</exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int? Height
    {
        get => _height;
        [SupportedOSPlatform("Windows")]
        init
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotSupportedException();
            
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            
            _height = value;
        }
    } 

    /// <summary>
    /// Gets or sets a Width of <see cref="Application"/> (in characters).
    /// </summary>
    /// <exception cref="NotSupportedException">Only Windows supports.</exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int? Width
    {
        get => _width;
        [SupportedOSPlatform("Windows")]
        init
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotSupportedException();
            
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
                
            _width = value;
        }
    }

    /// <summary>
    /// Specifies if it's okay to draw out of box. (Actually it's not possible by Application implementation).
    /// </summary>
    public bool BorderConflictsAllowed { get; init; } = true;

    /// <summary>
    /// Keys to change focus.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public ImmutableList<ConsoleKey> FocusChangeKeys
    {
        get => _focusChangeKeys;
        init => _focusChangeKeys = value ?? throw new ArgumentNullException(nameof(value));
    }

    // Now there is no need in overlapping disabled.
    public bool EnableOverlapping { get; init; } = true;

    // /// <summary>
    // /// Specifies if Drawer should make a delay to take all requests in small amount of time and only then draw. It's needed to smooth drawing to near-time requests.
    // /// </summary>
    // public bool DrawingDelay { get; init; } = true;

    /// <summary>
    /// A key to kill the application.
    /// </summary>
    public ConsoleKey? KillApplicationKey { get; init; } = null;
}
