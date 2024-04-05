// Tested type.

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// Handles pressed key.
/// </summary>
internal delegate void KeyPressedHandler(KeyPressedArgs args);

/// <summary>
/// Args of pressed key.
/// </summary>
/// <param name="KeyInfo"></param>
internal record KeyPressedArgs(ConsoleKeyInfo KeyInfo);

/// <summary>
/// Specifies <see cref="KeyListener"/> realization.
/// </summary>
internal record KeyListenerOptions;

/// <summary>
/// Listens console keys.
/// </summary>
internal class KeyListener
{
    private KeyPressedHandler? _keyPressed;

    private readonly CancellationTokenSource _cancellation = new();
    
    private readonly AutoResetEvent _waitEvent = new(false);

    // It may be used in the future, so it's important to get such parameter in constructor
    // ReSharper disable once NotAccessedField.Local
    private readonly KeyListenerOptions _options; 

    /// <summary>
    /// Indicates if this instance is running.
    /// </summary>
    public bool IsRunning { get; private set; } 

    /// <summary>
    /// Indicates if this instance is listening keys. (It can be stopped).
    /// </summary>
    public bool IsListening { get; private set; }
    
    /// <summary>
    /// Starts listening keys.
    /// </summary>
    /// <exception cref="KeyListeningException">It's already running.</exception>
    public void Start()
    {
        if (IsRunning)
        {
            throw new KeyListeningException("It's already running.");
        }

        // Thread listening keys.
        // It's not background to hold application running while it's running.
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

    /// <summary>
    /// Stops running.
    /// </summary>
    /// <exception cref="KeyListeningException">It's not running to stop it.</exception>
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

    /// <summary>
    /// Stops listening.
    /// </summary>
    public void ForceWait()
    {
        if (IsListening)
        {
            _waitEvent.Reset();
            IsListening = false;
        }
    }
    
    /// <summary>
    /// Restores listening.
    /// </summary>
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

    /// <summary>
    /// Creates an instance of <see cref="KeyListener"/>.
    /// </summary>
    /// <param name="options">Specifies <see cref="KeyListener"/> realization.</param>
    public KeyListener(KeyListenerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        
        _options = options;
    }
}