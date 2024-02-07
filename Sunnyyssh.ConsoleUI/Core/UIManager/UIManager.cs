using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace Sunnyyssh.ConsoleUI;

public abstract partial class UIManager
{
    protected readonly UIManagerSettings Settings;

    protected readonly FocusFlowManager HeadFocusFlowManager;

    public abstract int BufferWidth { get; }
    
    public abstract int BufferHeight { get; }
    
    // It can have different UIManager implementations.
    public static UIManager? Instance { get; private set; }

    [MemberNotNullWhen(true, nameof(Instance))]
    public static bool IsInitialized => Instance is not null;

    [MemberNotNullWhen(true, nameof(Instance))]
    public static bool IsRunning { get; private set; }

    [MemberNotNull(nameof(Instance))]
    public static UIManager Initialize(UIManagerSettings settings)
    {
        // Depending on settings different UIManager implementaions should be instantiated.
        
        // At this moment no specific implemenations are required .
        Instance = new DefaultUIManager(settings);
        
        return Instance;
    }

    public virtual void Run()
    {
        if (IsRunning)
        {
            throw new UIManagerException("The application is already running.");
        }

        IsRunning = true;

        DrawerOptions drawerOptions = new(
            Settings.DefaultBackground,
            Settings.DefaultForeground,
            Settings.ThrowOnBorderConflicts,
            false);
        Drawer.Start(drawerOptions);

        KeyListenerOptions keyListenerOptions = new();
        KeyListener.Start(keyListenerOptions);
        
        // FocusFlowManager should handle pressed keys.
        KeyListener.KeyPressed += HeadFocusFlowManager.HandlePressedKey; 
        
        Draw();
    }

    public virtual void Stop()
    {
        if (!IsRunning)
        {
            throw new UIManagerException("The application is not running.");
        }
        IsRunning = false;
        
        Drawer.Stop();
        
        // FocusFlowManager shouldn't handle pressed keys anymore.
        KeyListener.KeyPressed -= HeadFocusFlowManager.HandlePressedKey;
        KeyListener.Stop();
    }

    private protected abstract void Draw();

    private protected abstract void RedrawChild(UIElement child, RedrawElementEventArgs args);
    
    protected UIManager(UIManagerSettings settings)
    {
        Settings = settings;
        FocusManagerOptions focusManagerOptions = new (
            Settings.FocusChangeKeys,
            // Focus Flow should be looped.
            true,
            // If there are only one IFocusable nothing should happen when focus change is ought to occur.
            false);
        HeadFocusFlowManager = new FocusFlowManager(focusManagerOptions);
    }
}