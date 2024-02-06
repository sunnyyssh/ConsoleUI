// Tested type.

namespace Sunnyyssh.ConsoleUI;

public static class KeyListener
{
    private static KeyPressedHandler? _keyPressed;

    private static CancellationTokenSource _cancellation = new();

    private static AutoResetEvent _waitEvent = new(false);

    public static bool IsRunning { get; private set; } = false;

    public static bool IsListening { get; private set; } = false;
    
    public static void Start(KeyListenerOptions options)
    {
        if (IsRunning)
        {
            throw new KeyListeningException("It's already running.");
        }
        
        Thread runningThread = new Thread(() => StartWithCancellation(_cancellation.Token)) 
            { IsBackground = false };
        runningThread.Start();
        
        IsRunning = IsListening = true;
    }

    private static void StartWithCancellation(CancellationToken cancellationToken)
    {
        while (!_cancellation.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                // intercept parameter set to true in order not to show pressed key.
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                OnKeyPressed(keyInfo);
            }
            
            if (!IsListening)
            {
                _waitEvent.WaitOne();
            }
        }
    }

    public static void Stop()
    {
        if (!IsRunning)
        {
            throw new KeyListeningException("It's not running to stop it.");
        }
        
        _cancellation.Cancel();
        if (!IsListening)
        {
            _waitEvent.Set();
        }
    }

    public static void ForceWait()
    {
        if (IsListening)
        {
            _waitEvent.Reset();
            IsListening = false;
        }
    }
    
    public static void ForceRestore()
    {
        if (!IsListening)
        {
            _waitEvent.Set();
            IsListening = true;
        }
    }

    public static event KeyPressedHandler? KeyPressed
    {
        add
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            bool isNeededToRestore = IsRunning && (_keyPressed is null || _keyPressed.GetInvocationList().Length == 0);

            _keyPressed = (KeyPressedHandler)Delegate.Combine(_keyPressed, value);

            // If listening was stopped it's needed to restore it.
            if (isNeededToRestore)
            {
                ForceRestore();
            }
        }
        remove
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            _keyPressed = (KeyPressedHandler?)Delegate.Remove(_keyPressed, value);

            // If nobody listens then it's not needed to listen keys till someone subscribes event.
            bool isNeededToWait = IsRunning && (_keyPressed is null || _keyPressed.GetInvocationList().Length == 0);
            if (isNeededToWait)
            {
                ForceWait();
            }
        }
    }
    
    private static void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        KeyPressedArgs args = new(keyInfo);
        _keyPressed?.Invoke(args);
    }
}