using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public abstract class UIManager
{
    protected readonly UIManagerSettings Settings;

    private protected readonly FocusFlowManager HeadFocusFlowManager;

    private bool _hasStartedOnce = false;

    private readonly AutoResetEvent _waitForStopEvent = new (true);
    
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
        if (IsRunning)
        {
            throw new UIManagerException("Cannot initialize new instance of UIManager while current one is running. ");
        }
        
        // Depending on settings different UIManager implementaions should be instantiated.
        // At this moment no specific implemenations are required.
        Instance = new DefaultUIManager(settings);
        
        return Instance;
    }

    public void Run()
    {
        if (_hasStartedOnce)
        {
            throw new UIManagerException(
                "The application cannot be started multiple times. Once started it can only stop but not restart. ");
        }
        if (IsRunning)
        {
            throw new UIManagerException("The application is already running.");
        }
        IsRunning = true;
        _hasStartedOnce = true;
        
        // Making threads that are waiting for stop block.
        _waitForStopEvent.Reset();
        
        Drawer.Start();
        KeyListener.Start();
        HeadFocusFlowManager.TakeFocus();
        
        Draw();
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            throw new UIManagerException("The application is not running.");
        }
        
        HeadFocusFlowManager.LoseFocus();
        Drawer.Stop();
        KeyListener.Stop();
        
        IsRunning = false;
        
        // Allowing threads that are waiting for stop continue.
        _waitForStopEvent.Set();
    }

    public void Wait() => _waitForStopEvent.WaitOne();

    public bool AddChild(UIElement child, int left, int top)
        => AddChild(child, new Position(left, top));

    public bool AddChild(UIElement child, int left, double topRelational)
        => AddChild(child, new Position(left, topRelational));

    public bool AddChild(UIElement child, double leftRelational, int top)
        => AddChild(child, new Position(leftRelational, top));

    public bool AddChild(UIElement child, double leftRelational, double topRelational)
        => AddChild(child, new Position(leftRelational, topRelational));

    public bool AddChild(UIElement child, Position position)
    {
        if (!ElementsField.TryPlaceChild(child, position, out ChildInfo? childInfo))
            return false;

        child.RedrawElement += RedrawChild;
        if (child is IFocusable focusableChild)
        {
            HeadFocusFlowManager.Add(focusableChild); // TODO Here should be another way to order focusables then in order of adding.
        }
        
        if (IsRunning)
        {
            DrawChild(childInfo);
        }
        return true;
    }

    public bool RemoveChild(UIElement child)
    {
        if (!ElementsField.TryGetChild(child, out var childInfo))
            return false;
        
        child.RedrawElement -= RedrawChild;
        if (child is IFocusable focusableChild)
        {
            _ = HeadFocusFlowManager.TryRemove(focusableChild);
        }
        
        if (IsRunning)
        {
            EraseChild(childInfo);
        }

        return ElementsField.TryRemoveChild(child);
    }

    private protected abstract void Draw();

    private protected abstract void DrawChild(ChildInfo child);

    private protected abstract void EraseChild(ChildInfo child);

    private protected abstract void RedrawChild(UIElement child, RedrawElementEventArgs args);
    
    protected UIManager(UIManagerSettings settings)
    {
        Settings = settings;

        DrawerOptions drawerOptions = new(
            Settings.DefaultBackground,
            Settings.DefaultForeground,
            Settings.BorderConflictsAllowed) 
        {
            Width = Settings.Width,
            Height = Settings.Height
        };
        Drawer = new Drawer(drawerOptions);

        KeyListenerOptions keyListenerOptions = new();
        KeyListener = new KeyListener(keyListenerOptions);
        
        FocusManagerOptions focusManagerOptions = new (
            Settings.FocusChangeKeys,
            // Focus Flow should be looped.
            true,
            // If there are only one IFocusable nothing should happen when focus change is ought to occur.
            false,
            true,
            Settings.KillUIKey);
        HeadFocusFlowManager = new FocusFlowManager(focusManagerOptions);
        // FocusFlowManager should handle pressed keys.
        KeyListener.KeyPressed += HeadFocusFlowManager.HandlePressedKey;
        // When the Settings.KillUIKey is pressed UI should be killed.
        HeadFocusFlowManager.SpecialKeyPressed += _ => Stop();

        BufferWidth = Drawer.BufferWidth;
        BufferHeight = Drawer.BufferHeight;
        ElementsField = new ElementsField(BufferWidth, BufferHeight, Settings.EnableOverlapping);
    }
}