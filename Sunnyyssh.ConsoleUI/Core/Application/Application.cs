namespace Sunnyyssh.ConsoleUI;

public abstract class Application
{
    public static bool IsAnyAppRunning { get; private set; } 
    
    public ChildrenCollection Children { get; }
    
    private protected readonly ApplicationSettings Settings;

    private protected readonly FocusFlowManager HeadFocusFlowManager;

    private bool _hasStartedOnce = false;

    private readonly AutoResetEvent _waitForStopEvent = new (true);
    
    private protected readonly Drawer Drawer;

    private protected readonly KeyListener KeyListener;

    public int BufferWidth { get; }
    
    public int BufferHeight { get; }
    
    public bool IsRunning { get; private set; }

    public void Run()
    {
        if (_hasStartedOnce)
        {
            throw new ApplicationException(
                "The application cannot be started multiple times. Once started it can only stop but not restart. ");
        }
        
        if (IsRunning)
        {
            throw new ApplicationException("The application is already running.");
        }

        if (IsAnyAppRunning)
        {
            throw new ApplicationException("Another application is running now.");
        }
        
        IsRunning = true;
        IsAnyAppRunning = true;
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
            throw new ApplicationException("The application is not running.");
        }
        
        HeadFocusFlowManager.LoseFocus();
        Drawer.Stop();
        KeyListener.Stop();
        
        IsRunning = false;
        IsAnyAppRunning = false;
        
        // Allowing threads that are waiting for stop continue.
        _waitForStopEvent.Set();
    }

    public void Wait() => _waitForStopEvent.WaitOne();

    private void SubscribeChildren(ChildrenCollection orderedChildren)
    {
        foreach (var childInfo in orderedChildren)
        {
            SubscribeChild(childInfo);
        }
    }

    private void SubscribeChild(ChildInfo childInfo)
    {
        var child = childInfo.Child;
        
        child.RedrawElement += RedrawChild;
        
        if (child is IFocusable focusableChild)
        {
            HeadFocusFlowManager.Add(focusableChild);
        }
    }

    private protected abstract void Draw();

    private protected abstract void RedrawChild(UIElement child, RedrawElementEventArgs args);
    
    private protected Application(ApplicationSettings settings, ChildrenCollection orderedChildren)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(orderedChildren, nameof(orderedChildren));
        
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
            Settings.KillApplicationKey);
        HeadFocusFlowManager = new FocusFlowManager(focusManagerOptions);
        // FocusFlowManager should handle pressed keys.
        KeyListener.KeyPressed += HeadFocusFlowManager.HandlePressedKey;
        // When the Settings.KillUIKey is pressed UI should be killed.
        HeadFocusFlowManager.SpecialKeyPressed += _ => Stop();

        BufferWidth = Drawer.BufferWidth;
        BufferHeight = Drawer.BufferHeight;
        
        Children = orderedChildren;
        SubscribeChildren(Children);
    }
}