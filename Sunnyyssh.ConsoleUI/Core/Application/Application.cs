using System.Collections.Immutable;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Handles the whole UI. It draws and listens keys in different threads and holds application running while it's running.
/// </summary>
/// <example>
/// <code>
/// // ... (ApplicationBuilder) appBuilder initialization.
/// 
/// Application app = appBuilder.Build();
///
/// app.Run();
/// 
/// app.Wait(); // Waiting for app.
/// </code>
/// </example>
public abstract class Application
{
    /// <summary>
    /// Indicates if any <see cref="Application"/> instance is running at this moment.
    /// </summary>
    public static bool IsAnyAppRunning { get; private set; } 
    
    /// <summary>
    /// Collection of <see cref="ChildInfo"/> instances holding <see cref="UIElement"/> children.
    /// </summary>
    public IReadOnlyList<ChildInfo> Children { get; }
    
    private protected readonly ApplicationSettings Settings;

    private protected readonly FocusFlowManager HeadFocusFlowManager;

    // Indicates if this instance was started in past.
    private bool _hasStartedOnce = false;

    // Helps wait for running application.
    private readonly ManualResetEvent _waitForStopEvent = new (true);
    
    private protected readonly Drawer Drawer;

    private protected readonly KeyListener KeyListener;

    /// <summary>
    /// The width of area where it can draw.
    /// </summary>
    public int BufferWidth { get; }
    
    /// <summary>
    /// The height of area where it can draw.
    /// </summary>
    public int BufferHeight { get; }
    
    /// <summary>
    /// Indicates if this instance is running now.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Starts UI running.
    /// </summary>
    /// <exception cref="ApplicationException">Trying to run incorrectly.</exception>
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
        // It should take focus only after Drawer and KeyListener start because it forces focus flow immediately.
        HeadFocusFlowManager.TakeFocus();
        
        Draw();
    }

    /// <summary>
    /// Stops UI running.
    /// </summary>
    /// <exception cref="ApplicationException"></exception>
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

    /// <summary>
    /// Waits for running UI. Allows calling thread continue only when <see cref="Application"/> is not running.
    /// </summary>
    public void Wait() => _waitForStopEvent.WaitOne();
    
    private void SubscribeChildren(IReadOnlyList<ChildInfo> orderedChildren)
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
    }

    /// <summary>
    /// Draws the whole UI of application at start.
    /// </summary>
    private protected abstract void Draw();

    /// <summary>
    /// Redraws the child with given args.
    /// </summary>
    /// <param name="child">Child to redraw.</param>
    /// <param name="args">Redraw args.</param>
    private protected abstract void RedrawChild(UIElement child, RedrawElementEventArgs args);
    
    private protected Application(ApplicationSettings settings, ImmutableList<ChildInfo> orderedChildren, FocusFlowSpecification focusFlowSpecification)
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
            focusFlowSpecification,
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