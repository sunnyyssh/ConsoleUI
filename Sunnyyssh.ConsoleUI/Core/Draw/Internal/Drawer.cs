using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// The options of <see cref="Drawer"/>. They give opportuninty to custom drawing process.
/// </summary>
/// <param name="DefaultBackground">The default background color</param>
/// <param name="DefaultForeground">The default foreground color</param>
/// <param name="BorderConflictsAllowed">Indicates if it's expected to throw an exception on trying to draw outside the buffer.</param>
/// <param name="RequestsNotRunningAllowed">
/// Indicates if it's expected to throw an exception when new request is enqueued when Drawer is not running.
/// </param>
internal record DrawerOptions(Color DefaultBackground, Color DefaultForeground, bool BorderConflictsAllowed,
    bool RequestsNotRunningAllowed = false)
{
    /// <summary>
    /// The width of drawing field. Fully ignored when running OS is not windows.
    /// </summary>
    public int? Width { get; init; }
    
    /// <summary>
    /// The height of drawing field. Fully ignored when running OS is not windows.
    /// </summary>
    public int? Height { get; init; }
}

/// <summary>
/// Provides all drawing process in the specialized drawing thread.
/// </summary>
internal class Drawer
{
    // All drawing directly in console is incapsulated in DrawerPal class
    private DrawerPal _drawerPal;

    // When Drawer.Stop() method is invoked this should be cancelled
    // to cancel actual drawing operations and make incoming requests be not drawn.
    private readonly CancellationTokenSource _cancellation = new();
    
    // When request to draw InternalDrawState occurs it is enqueued in this queue.
    // This collection gives an oportunity to wait for requests when the actual queue is empty.
    private readonly RequestsQueue<InternalDrawState> _drawRequestsQueue = new();
    
    private readonly DrawerOptions _options;

    /// <summary>
    /// Indicates if Drawer has been already started.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// The width of buffer where drawer can actually draw. 
    /// </summary>
    public int BufferWidth => _drawerPal.BufferWidth;

    /// <summary>
    /// The height of buffer where drawer can actually draw. 
    /// </summary>
    public int BufferHeight => _drawerPal.BufferHeight;
    
    /// <summary>
    /// Enqueues a request to draw the state.
    /// It's dequeued and drawn when current drawing iteration ends
    /// or immediately if it's not drawing at the moment.
    /// </summary>
    /// <param name="drawState">The state to enqueue to draw.</param>
    public void EnqueueRequest(InternalDrawState drawState)
    {
        if (!IsRunning && !_options.RequestsNotRunningAllowed)
        {
            throw new DrawingException("Drawer is not running."); // BUG: when program exits something goes wrong.
        }
        ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
        
        _drawRequestsQueue.Enqueue(drawState);
    }
    
    /// <summary>
    /// Starts drawing process in the another thread.
    /// </summary>
    /// <exception cref="DrawingException"></exception>
    public void Start()
    {
        if (IsRunning)
            throw new DrawingException("It's already running.");
        
        // All drawing iterations must be performed in the another thread
        // not to hold the calling thread while drawing.
        Thread drawingThread = new Thread(() =>
        {
            RunWithCancellation(_cancellation.Token);
        })
        {
            // false because this thread should hold the app running.
            IsBackground = false,
        };
        drawingThread.Start();
        
        IsRunning = true;
    }

    // In dependence on options different DrawerPal instances can be initialized.
    // The realization may differ depending on platform, specific options etc.
    [MemberNotNull(nameof(_drawerPal))]
    private void InitializeDrawerPal(DrawerOptions options)
    {
        // If application is running on windows then DrawerPal implementation is ought to be specified for windows.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _drawerPal = new WindowsDrawerPal(
                options.DefaultBackground,
                options.DefaultForeground,
                options.BorderConflictsAllowed,
                options.Width,
                options.Height);
            return;
        }
        
        // If there are no reason to use specific implementations of DrawerPal
        // the default DrawerPal implementations should be used.  
        _drawerPal = new DrawerPal(options.DefaultBackground, options.DefaultForeground, options.BorderConflictsAllowed); // Default DrawerPal.
    }
    
    // Method invoked in the drawing thread. 
    // It loops the drawing iterations while it's cancelled
    // and waits for requests ot cancellation when no requests are in queue.
    private void RunWithCancellation(CancellationToken cancellationToken)
    {
        // Drawing default background.
        FillConsoleWithColor(_options.DefaultBackground, cancellationToken);
        // Making DrawerPal instance handle starting drawing process.
        _drawerPal.OnStart();
        
        // Looping while it's not cancelled.
        while (!cancellationToken.IsCancellationRequested)
        {
            DrawRequests(cancellationToken);
            
            // Waiting for the request or the cancellation.
            _drawRequestsQueue.WaitForRequests();
        }
    }

    // Fills console with color.
    private void FillConsoleWithColor(Color color, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        _drawerPal.Clear();
        int height = _drawerPal.BufferHeight;
        int width = _drawerPal.BufferWidth;
        
        // Creating the state of the whole console one-colored
        InternalDrawState drawState = new (
            Enumerable.Range(0, height).Select(top => new PixelLine(0, top,
                Enumerable.Range(0, width)
                    .Select(_ => new PixelInfo(color))
                    .ToArray())
            ).ToArray()
        );
        
        // It's important not to enqueue this request but directly draw it
        // because it can overlap already enqueued requests, what is fully unexpected.
        DrawSingleRequest(drawState, cancellationToken);
    }
    
    public void Stop()
    {
        if (!IsRunning)
            throw new DrawingException("It's not running.");

        // It's necessary to cancel before exiting waiting
        // because otherwise it goes to the another iteration and waits again before it canceles 
        _cancellation.Cancel();
        // If drawing thread is waiting for requests
        // we force stopping waiting
        // and make the drawing thread finish its work.
        // Look void RunWithCancellation(CancellationToken)
        _drawRequestsQueue.ForceStopWaiting();
        
        IsRunning = false;
    }
    
    // Dequeues and draws all enqueued requests.
    private void DrawRequests(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        if (_drawRequestsQueue.IsEmpty)
            return;    
        
        // Dequeueing requests into ordered collection.
        var allRequests = _drawRequestsQueue.DequeueAll();

        // All requests should be combined into one.
        // What's important, later states are expected to overlap the earlier ones.
        var combinedRequest = InternalDrawState.Combine(allRequests); 

        DrawSingleRequest(combinedRequest, cancellationToken);
    }

    private void DrawSingleRequest(InternalDrawState drawState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        _drawerPal.DrawSingleRequest(drawState, cancellationToken);
    }

    /// <summary>
    /// Creates an instance of <see cref="Drawer"/> with specified options.
    /// </summary>
    /// <param name="options">Specific drawing options.</param>
    public Drawer(DrawerOptions options)
    {
        InitializeDrawerPal(options);
        _options = options;
    }
}