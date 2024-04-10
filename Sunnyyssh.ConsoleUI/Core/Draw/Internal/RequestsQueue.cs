using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Sunnyyssh.ConsoleUI;

/// <summary>
/// The queue with ability to wait requests when there are no queued.
/// </summary>
/// <typeparam name="T">The type of request.</typeparam>
internal class RequestsQueue<T> : IEnumerable<T>
{
    private readonly ManualResetEvent _requestWaitEvent = new(false);

    private readonly ConcurrentQueue<T> _requestsQueue = new();

    public bool IsEmpty => _requestsQueue.IsEmpty;
    
    /// <summary>
    /// Enqueues request and makes waiting threads run.
    /// </summary>
    /// <param name="request">The request to enqueue.</param>
    public void Enqueue(T request)
    {
        bool isNeededToSetEvent = _requestsQueue.IsEmpty;
        _requestsQueue.Enqueue(request);
        
        if (isNeededToSetEvent)
        {
            _requestWaitEvent.Set();
        }
    }

    /// <summary>
    /// Dequeues all requests and makes waiting threads wait for new request.
    /// </summary>
    /// <returns>An array with dequeued requests.</returns>
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

    /// <summary>
    /// Tries to dequeue request. If request is single it makes waiting threads wait for new one.
    /// </summary>
    /// <param name="request">Dequeued request.</param>
    /// <returns>True if there was a request. False otherwise.</returns>
    public bool TryDequeue([MaybeNullWhen(false)] out T request)
    {
        bool ifRequestReturned = _requestsQueue.TryDequeue(out request);
        
        if (ifRequestReturned && _requestsQueue.IsEmpty)
        {
            // Makes waiting threads wait.
            _requestWaitEvent.Reset();
        }

        return ifRequestReturned;
    }

    /// <summary>
    /// Tries to peek request.
    /// </summary>
    /// <param name="request">Peeked request.</param>
    /// <returns>True if there was a request. False otherwise.</returns>
    public bool TryPeek([MaybeNullWhen(false)] out T request)
    {
        return _requestsQueue.TryPeek(out request);
    }

    /// <summary>
    /// Waits for new request if there are no requests in queue.
    /// </summary>
    public void WaitForRequests()
    {
        _requestWaitEvent.WaitOne();
    }

    /// <summary>
    /// Forces waiting threads continue running.
    /// </summary>
    public void ForceStopWaiting()
    {
        _requestWaitEvent.Set();
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return _requestsQueue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_requestsQueue).GetEnumerator();
    }
}