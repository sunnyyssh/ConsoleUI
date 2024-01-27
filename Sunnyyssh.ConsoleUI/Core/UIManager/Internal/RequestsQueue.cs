// Tested type.

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

public class RequestsQueue<T> : IEnumerable<T>
{
    private readonly AutoResetEvent _requestWaitEvent = new AutoResetEvent(false);

    private readonly ConcurrentQueue<T> _requstsQueue = new ConcurrentQueue<T>();

    public bool IsEmpty => _requstsQueue.IsEmpty;
    
    public void Enqueue(T request)
    {
        bool isNeededToSetEvent = _requstsQueue.IsEmpty;
        _requstsQueue.Enqueue(request);
        
        if (isNeededToSetEvent)
        {
            _requestWaitEvent.Set();
        }
    }

    public T[] DequeueAll()
    {
        IEnumerable<T> DequeueAllEnumerable()
        {
            while (TryDequeue(out var request))
            {
                yield return request;
            }
        }

        return DequeueAllEnumerable().ToArray();
    }

    public bool TryDequeue([MaybeNullWhen(false)] out T request)
    {
        bool ifRequestReturned = _requstsQueue.TryDequeue(out request);
        
        if (ifRequestReturned && _requstsQueue.IsEmpty)
        {
            _requestWaitEvent.Reset();
        }

        return ifRequestReturned;
    }

    public bool TryPop([MaybeNullWhen(false)] out T request)
    {
        return _requstsQueue.TryPeek(out request);
    }

    public void WaitForRequests()
    {
        _requestWaitEvent.WaitOne();
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return _requstsQueue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_requstsQueue).GetEnumerator();
    }
}