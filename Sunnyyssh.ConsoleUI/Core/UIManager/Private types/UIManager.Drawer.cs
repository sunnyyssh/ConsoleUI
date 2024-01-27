namespace Sunnyyssh.ConsoleUI;

partial class UIManager
{
    protected static class Drawer
    {
        public static void StartWithCancellation(CancellationToken cancellationToken)
        {
            Thread drawingThread = new Thread(() =>
            {
                RunWithCancellation(cancellationToken);
            })
            {
                IsBackground = false,
                
            };
            drawingThread.Start();
        }

        private static void RunWithCancellation(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                
            }
        }
    }
}