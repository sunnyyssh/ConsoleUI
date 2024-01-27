using System.Collections.Concurrent;
using Sunnyyssh.ConsoleUI;

namespace Sunnyyssh.ConsoleUI;

partial class UIManager
{
    protected static class Drawer
    {
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private static readonly RequestsQueue<InternalDrawState> DrawRequestsQueue = new RequestsQueue<InternalDrawState>();

        public static void EnqueueRequest(InternalDrawState drawState)
        {
            ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
            
            DrawRequestsQueue.Enqueue(drawState);
        }
        
        public static void StartWithCancellation(CancellationToken cancellationToken)
        {
            if (IsRunning)
            {
                // TODO throw the exception.
            }
            Thread drawingThread = new Thread(() =>
            {
                RunWithCancellation(cancellationToken);
            })
            {
                // false because this thread should hold the app running.
                IsBackground = false,
                
            };
            drawingThread.Start();
            IsRunning = true;
        }
        
        private static void RunWithCancellation(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                DrawRequests();
                
                DrawRequestsQueue.WaitForRequests();
                // Waiting for the request.
            }
        }

        private static void DrawRequests()
        {
            if (DrawRequestsQueue.IsEmpty)
                return;    
            
            var allRequests = DrawRequestsQueue.DequeueAll();

            var combinedRequest = InternalDrawState.Combine(allRequests);

            DrawSingleRequest(combinedRequest);
        }

        private static void DrawSingleRequest(InternalDrawState drawState)
        {
            throw new NotImplementedException();
        }
    }
}