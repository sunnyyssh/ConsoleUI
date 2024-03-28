using System.Runtime.Versioning;

namespace Sunnyyssh.ConsoleUI;

public sealed class ApplicationSettings
{
    private readonly Color _defaultBackground = Color.Black;
    private readonly Color _defaultForeground = Color.White;
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
    
    public int? Height { get; [SupportedOSPlatform("Windows")] init; } = null; // TODO
    
    public int? Width { get; [SupportedOSPlatform("Windows")] init; } = null;

    public bool BorderConflictsAllowed { get; init; } = true;

    public ConsoleKey[] FocusChangeKeys { get; init; } = new[] { ConsoleKey.Tab };

    // Now there is no need in overlapping disabled.
    public bool EnableOverlapping { get; private init; } = true;

    // TODO make DrawerPal draw with delay to get all requests to redraw.
    public bool DrawingDelay { get; init; } = true; // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public ConsoleKey? KillApplicationKey { get; init; } = null;
}
