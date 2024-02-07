// Tested core implementation.
// Not Tested actual drawing.

namespace Sunnyyssh.ConsoleUI;
    
internal static class Drawer
{
    private static DrawerPal DrawerPal;

    private static readonly CancellationTokenSource Cancellation = new();

    /// <summary>
    /// 
    /// </summary>
    private static readonly RequestsQueue<InternalDrawState> DrawRequestsQueue = new();
    
    public static bool IsRunning { get; private set; }

    public static int BufferWidth => DrawerPal.BufferWidth;

    public static int BufferHeight => DrawerPal.BufferHeight;
    
    public static void EnqueueRequest(InternalDrawState drawState)
    {
        if (!IsRunning)
        {
            throw new DrawingException("Drawer is not running.");
        }
        ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
        
        DrawRequestsQueue.Enqueue(drawState);
    }
    
    public static void Start(DrawerOptions options)
    {
        if (IsRunning)
        {
            throw new DrawingException("It's already running.");
        }
        
        InitializeDrawerPal(options);
        
        Thread drawingThread = new Thread(() =>
        {
            RunWithCancellation(Cancellation.Token);
        })
        {
            // false because this thread should hold the app running.
            IsBackground = false,
        };
        drawingThread.Start();
        IsRunning = true;
    }

    private static void InitializeDrawerPal(DrawerOptions options)
    {
        // In dependence on options different DrawerPal instances can be initialized.
        // The realization may differ depending on platform, specific options etc.
        DrawerPal = new DrawerPal(options.DefaultBackground, options.DefaultForeground, options.ThrowOnBorderConflicts); // Default DrawerPal.
    }
    
    private static void RunWithCancellation(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DrawRequests(cancellationToken);
            
            // Waiting for the request.
            DrawRequestsQueue.WaitForRequests();
        }
    }

    public static void Stop()
    {
        if (!IsRunning)
        {
            throw new DrawingException("It's not running.");
        }
        
        // It's necessary to cancel before exiting waiting
        // because otherwise it goes to the another iteration and waits again before it canceles 
        Cancellation.Cancel();
        // If drawing thread is waiting for requests
        // we force stopping waiting
        // and make the drawing thread finish its work.
        // Look void RunWithCancellation(CancellationToken)
        DrawRequestsQueue.ForceStopWaiting();
    }
    
    private static void DrawRequests(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        if (DrawRequestsQueue.IsEmpty)
            return;    
        
        var allRequests = DrawRequestsQueue.DequeueAll();

        // It's guaranteed that every line has unique Top value.
        // It's well for optimization of the drawing process.
        var combinedRequest = InternalDrawState.Combine(allRequests); 

        DrawSingleRequest(combinedRequest, cancellationToken);
    }

    private static void DrawSingleRequest(InternalDrawState drawState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        // WATCH NOTES !!! 
        DrawerPal.DrawSingleRequest(drawState, cancellationToken);
    }
}