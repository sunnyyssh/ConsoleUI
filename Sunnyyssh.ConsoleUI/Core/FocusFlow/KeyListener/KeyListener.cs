// Tested type.

namespace Sunnyyssh.ConsoleUI;

public delegate void KeyPressedHandler(KeyPressedArgs args);

public record KeyPressedArgs(ConsoleKeyInfo KeyInfo);

public record KeyListenerOptions;

public class KeyListener
{
    private KeyPressedHandler? _keyPressed;

    private readonly CancellationTokenSource _cancellation = new();

    private readonly AutoResetEvent _waitEvent = new(false);

    // It may be used in the future, so it's important to get such parameter in constructor
    private readonly KeyListenerOptions _options; 

    public bool IsRunning { get; private set; } 

    public bool IsListening { get; private set; }
    
    public void Start()
    {
        if (IsRunning)
        {
            throw new KeyListeningException("It's already running.");
        }

        Thread runningThread = new(
                () => StartWithCancellation(_cancellation.Token))
            { IsBackground = false };
        runningThread.Start();
        
        IsRunning = IsListening = true;
    }

    private void StartWithCancellation(CancellationToken cancellationToken)
    {
        Console.CursorVisible = false;
        
        while (!cancellationToken.IsCancellationRequested)
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

    public void Stop()
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

        IsRunning = false;
    }

    public void ForceWait()
    {
        if (IsListening)
        {
            _waitEvent.Reset();
            IsListening = false;
        }
    }
    
    public void ForceRestore()
    {
        if (!IsListening)
        {
            _waitEvent.Set();
            IsListening = true;
        }
    }

    public event KeyPressedHandler? KeyPressed
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
    
    private void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        KeyPressedArgs args = new(keyInfo);
        _keyPressed?.Invoke(args);
    }

    public KeyListener(KeyListenerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        
        _options = options;
    }
}