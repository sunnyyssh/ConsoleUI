namespace Sunnyyssh.ConsoleUI.KeyListener;

internal static class KeyListener
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

            if (isNeededToRestore)
            {
                ForceRestore();
            }
        }
        remove
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            _keyPressed = (KeyPressedHandler?)Delegate.Remove(_keyPressed, value);

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