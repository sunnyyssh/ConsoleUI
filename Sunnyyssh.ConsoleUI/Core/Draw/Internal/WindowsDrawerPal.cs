using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// <see cref="DrawerPal"/> implementation for Windows. Supports specifying width and height.
/// </summary>
[SupportedOSPlatform("Windows")]
internal sealed class WindowsDrawerPal : DrawerPal
{
    private readonly int? _initializedWidth;
    private readonly int? _initializedHeight;
    public WindowsDrawerPal(Color defaultBackground, Color defaultForeground, bool borderConflictsAllowed, 
        int? width, int? height) : 
        base(defaultBackground, defaultForeground, borderConflictsAllowed)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new NotSupportedException();
        
        _initializedWidth = width;
        _initializedHeight = height;
    }

    /// <summary>
    /// <inheritdoc cref="DrawerPal.OnStart"/>
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();
        if (_initializedWidth.HasValue)
        {
            Console.WindowWidth = _initializedWidth.Value;
        }
        if (_initializedHeight.HasValue)
        {
            Console.WindowHeight = _initializedHeight.Value;
        }
    }
}