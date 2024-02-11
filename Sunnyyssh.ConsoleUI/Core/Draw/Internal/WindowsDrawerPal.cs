using System.Runtime.Versioning;

namespace Sunnyyssh.ConsoleUI;

[SupportedOSPlatform("Windows")]
internal sealed class WindowsDrawerPal : DrawerPal
{
    private readonly int? _initializedWidth;
    private readonly int? _initializedHeight;
    public WindowsDrawerPal(Color defaultBackground, Color defaultForeground, bool throwOnBorderConflict, 
        int? width, int? height) : 
        base(defaultBackground, defaultForeground, throwOnBorderConflict)
    {
        _initializedWidth = width;
        _initializedHeight = height;
    }

    public override void OnStart()
    {
        base.OnStart();
        if (_initializedWidth.HasValue)
        {
            Console.WindowWidth = _initializedWidth.Value;
        }
        if (_initializedHeight.HasValue)
        {
            Console.WindowWidth = _initializedHeight.Value;
        }
    }
}