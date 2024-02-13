// Tested core implementation.
// Not Tested actual drawing.

using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

internal record DrawerOptions(Color DefaultBackground, Color DefaultForeground, bool ThrowOnBorderConflicts, 
    // Width and Height will be fully ignored if it's not Windows. (Actually is it's not Windows they mustn't have values).
    int? Width, int? Height,
    bool ThrowOnRequestWhileNotRunning = false);

internal class Drawer
{
    private DrawerPal _drawerPal;

    private readonly CancellationTokenSource _cancellation = new();

    /// <summary>
    /// 
    /// </summary>
    private readonly RequestsQueue<InternalDrawState> _drawRequestsQueue = new();

    private readonly DrawerOptions _options;

    public bool IsRunning { get; private set; }

    public int BufferWidth => _drawerPal.BufferWidth;

    public int BufferHeight => _drawerPal.BufferHeight;
    
    public void EnqueueRequest(InternalDrawState drawState)
    {
        if (!IsRunning && _options.ThrowOnRequestWhileNotRunning)
        {
            throw new DrawingException("Drawer is not running.");
        }
        ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
        
        _drawRequestsQueue.Enqueue(drawState);
    }
    
    public void Start()
    {
        if (IsRunning)
        {
            throw new DrawingException("It's already running.");
        }
        
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

    [MemberNotNull(nameof(_drawerPal))]
    private void InitializeDrawerPal(DrawerOptions options)
    {
        // In dependence on options different DrawerPal instances can be initialized.
        // The realization may differ depending on platform, specific options etc.
        _drawerPal = new DrawerPal(options.DefaultBackground, options.DefaultForeground, options.ThrowOnBorderConflicts); // Default DrawerPal.
    }
    
    private void RunWithCancellation(CancellationToken cancellationToken)
    {
        FillConsoleWithColor(_options.DefaultBackground, cancellationToken);
        _drawerPal.OnStart();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            DrawRequests(cancellationToken);
            
            // Waiting for the request.
            _drawRequestsQueue.WaitForRequests();
        }
    }

    private void FillConsoleWithColor(Color color, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        _drawerPal.Clear();
        int height = _drawerPal.BufferHeight;
        int width = _drawerPal.BufferWidth;
        
        // Creating the state of whole console one-colored
        InternalDrawState drawState = new (
            Enumerable.Range(0, height).Select(top => new PixelLine(0, top,
                Enumerable.Range(0, width)
                    .Select(_ => new PixelInfo(color))
                    .ToArray())
            ).ToArray()
        );
        
        DrawSingleRequest(drawState, cancellationToken);
    }
    
    public void Stop()
    {
        if (!IsRunning)
        {
            throw new DrawingException("It's not running.");
        }

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
    
    private void DrawRequests(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        if (_drawRequestsQueue.IsEmpty)
            return;    
        
        var allRequests = _drawRequestsQueue.DequeueAll();

        // It's guaranteed that every line has unique Top value.
        // It's well for optimization of the drawing process.
        var combinedRequest = InternalDrawState.Combine(allRequests); 

        DrawSingleRequest(combinedRequest, cancellationToken);
    }

    private void DrawSingleRequest(InternalDrawState drawState, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;
        
        _drawerPal.DrawSingleRequest(drawState, cancellationToken);
    }

    public Drawer(DrawerOptions options)
    {
        InitializeDrawerPal(options);
        _options = options;
    }
}