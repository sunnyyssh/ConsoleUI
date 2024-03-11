using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI.Handlers;

public class TimeCounter : IDisposable
{
    private readonly int _maxMilliseconds;

    private readonly Stopwatch _runner = new();

    private bool _disposed;

    public TimeCounter(int maxMilliseconds)
    {
        _maxMilliseconds = maxMilliseconds;
    }

    public void Start()
    {
        if (_disposed)
        {
            throw new InvalidOperationException();
        }
        
        _runner.Start();
    }

    public bool HasReachedMax
    {
        get
        {
            if (_disposed)
            {
                throw new InvalidOperationException();
            }

            return _runner.ElapsedMilliseconds >= _maxMilliseconds;
        }
    } 
    
    public void Dispose()
    {
        if (_disposed)
        {
            throw new InvalidOperationException();
        }
        
        if (_runner.IsRunning)
        {
            _runner.Stop();
        }

        _disposed = true;
    }

    ~TimeCounter()
    {
        if (!_disposed)
        {
            Dispose();
        }
    }
}