// Tested core implementation.
// Not Tested actual drawing.

namespace Sunnyyssh.ConsoleUI;

partial class UIManager
{
    //TODO make it protected.
    public static class Drawer
    {
        public static bool IsRunning { get; private set; }

        private static readonly CancellationTokenSource Cancellation = new CancellationTokenSource();

        /// <summary>
        /// 
        /// </summary>
        private static readonly RequestsQueue<InternalDrawState> DrawRequestsQueue = new RequestsQueue<InternalDrawState>();

        public static void EnqueueRequest(InternalDrawState drawState)
        {
            ArgumentNullException.ThrowIfNull(drawState, nameof(drawState));
            
            DrawRequestsQueue.Enqueue(drawState);
        }
        
        public static void Start()
        {
            if (IsRunning)
            {
                // TODO throw the exception.
            }
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
        
        private static void RunWithCancellation(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                DrawRequests(cancellationToken);
                
                // Waiting for the request.
                DrawRequestsQueue.WaitForRequests();
            }

            Console.WriteLine("Really stopped");
        }

        public static void Stop()
        {
            if (!IsRunning)
            {
                // TODO throw an exception.
            }
            
            // It's necessary to cancel before exiting waiting
            // because otherwise it goes to the another iteration and waits again before it canceles 
            Cancellation.Cancel();
            DrawRequestsQueue.ForceStopWaiting();
        }
        
        private static void DrawRequests(CancellationToken cancellationToken)
        {
            if (DrawRequestsQueue.IsEmpty)
                return;    
            
            var allRequests = DrawRequestsQueue.DequeueAll();

            var combinedRequest = InternalDrawState.Combine(allRequests);

            DrawSingleRequest(combinedRequest, cancellationToken);
        }

        private static void DrawSingleRequest(InternalDrawState drawState, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            
            throw new NotImplementedException();
        }
    }
}