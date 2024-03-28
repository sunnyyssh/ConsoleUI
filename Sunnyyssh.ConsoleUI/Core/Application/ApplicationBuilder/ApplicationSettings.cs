using System.Runtime.Versioning;

namespace Sunnyyssh.ConsoleUI;

public sealed class ApplicationSettings
{
    private readonly Color _defaultBackground = Color.Black;
    private readonly Color _defaultForeground = Color.White;
    private readonly ConsoleKey[] _focusChangeKeys = new[] { ConsoleKey.Tab };
    private readonly int? _height = null;
    private readonly int? _width = null;

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

    public int? Height
    {
        get => _height;
        [SupportedOSPlatform("Windows")]
        init
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            
            _height = value;
        }
    } // TODO

    public int? Width
    {
        get => _width;
        [SupportedOSPlatform("Windows")]
        init
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
                
            _width = value;
        }
    }

    public bool BorderConflictsAllowed { get; init; } = true;

    public ConsoleKey[] FocusChangeKeys
    {
        get => _focusChangeKeys;
        init => _focusChangeKeys = value ?? throw new ArgumentNullException(nameof(value));
    }

    // Now there is no need in overlapping disabled.
    public bool EnableOverlapping { get; private init; } = true;

    // TODO make DrawerPal draw with delay to get all requests to redraw.
    public bool DrawingDelay { get; init; } = true; // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public ConsoleKey? KillApplicationKey { get; init; } = null;
}
