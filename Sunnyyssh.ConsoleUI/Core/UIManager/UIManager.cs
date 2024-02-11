using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace Sunnyyssh.ConsoleUI;

public abstract partial class UIManager
{
    protected readonly UIManagerSettings Settings;

    protected readonly FocusFlowManager HeadFocusFlowManager;

    private protected readonly Drawer Drawer;

    private protected readonly KeyListener KeyListener;

    private protected readonly ElementsField ElementsField;

    public int BufferWidth { get; }
    
    public int BufferHeight { get; }
    
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

    public void Run()
    {
        if (IsRunning)
        {
            throw new UIManagerException("The application is already running.");
        }
        IsRunning = true;
        
        Drawer.Start();
        
        KeyListener.Start();
        // FocusFlowManager should handle pressed keys.
        KeyListener.KeyPressed += HeadFocusFlowManager.HandlePressedKey; 
        
        HeadFocusFlowManager.TakeFocus();
        
        Draw();
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            throw new UIManagerException("The application is not running.");
        }
        IsRunning = false;
        
        HeadFocusFlowManager.LoseFocus();
        
        Drawer.Stop();
        
        // FocusFlowManager shouldn't handle pressed keys anymore.
        KeyListener.KeyPressed -= HeadFocusFlowManager.HandlePressedKey;
        KeyListener.Stop();
    }

    public bool TryAddChild(UIElement child, Position position)
    {
        if (!ElementsField.TryPlaceChild(child, position, out ChildInfo? childInfo))
            return false;

        child.RedrawElement += RedrawChild;
        if (child is IFocusable focusableChild)
        {
            HeadFocusFlowManager.Add(focusableChild);
        }
        
        if (IsRunning)
        {
            DrawChild(childInfo);
        }

        return true;
    }

    private protected abstract void Draw();

    private protected abstract void DrawChild(ChildInfo child);

    private protected abstract void RedrawChild(UIElement child, RedrawElementEventArgs args); // handle overlapping !!!
    
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
        
        DrawerOptions drawerOptions = new(
            Settings.DefaultBackground,
            Settings.DefaultForeground,
            Settings.ThrowOnBorderConflicts,
            Settings.Width,
            Settings.Height);
        Drawer = new Drawer(drawerOptions);

        KeyListenerOptions keyListenerOptions = new();
        KeyListener = new KeyListener(keyListenerOptions);

        BufferWidth = Drawer.BufferWidth;
        BufferHeight = Drawer.BufferHeight;
        ElementsField = new ElementsField(BufferWidth, BufferHeight, Settings.EnableOverlapping);
    }
}