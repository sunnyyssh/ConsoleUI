using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

public abstract partial class UIManager
{
    
    private static UIManager? _instance;

    protected CancellationTokenSource Cancellation { get; } = new CancellationTokenSource();

    public static UIManager? Instance
    {
        [DebuggerStepThrough] get => _instance;
        [DebuggerStepThrough] private set => _instance = value ?? throw new ArgumentNullException();
    }
    protected UIManagerSettings Settings { get; private init; }
    
    private protected FocusManager FocusManager { get; private init; }

    public static bool IsInitialized => _instance is not null;

    public bool IsRunning { get; private set; }

    public int Width => Console.BufferWidth;

    public int Height => Console.BufferHeight;

    protected List<ChildInfo> ChildrenList { get; private init; } = new List<ChildInfo>();
    
    public UIElement[] Children => ChildrenList.Select(c => c.Child).ToArray();

    #region Instance methods

    public void AddChild(UIElement child)
    {
        ResolveChildSizeAndPosition(child, out int left, out int top, out int height, out int width);
        ChildInfo childInfo = new ChildInfo(child, left, top, height, width);
        
        ChildrenList.Add(childInfo);
        
        SubscribeChildEvents(child);
        
        if (IsRunning)
        {
            child.OnDraw();
            DrawChild(childInfo);
        }
    }

    public void RemoveChild(UIElement child)
    {
        ChildrenList.RemoveAll(c => c.Child == child);
        if (IsRunning)
        {
            child.OnRemove();
            EraseChild(child);
        }
    }

    public void Run()
    {
        if (IsRunning)
        {
            throw new IncorrectRunException("Console UI is already running.");
        }

        RunWithCancellation(Cancellation.Token);
        
    }

    public void Exit()
    {
        Cancellation.Cancel();
        throw new NotImplementedException();
    }

    private void SubscribeChildEvents(UIElement child)
    {
        child.RedrawElement += RedrawChild;
        child.RemoveElement += RemoveChild;
        if (child is IFocusable focusableChild)
        {
            //focusableChild.ForceEnteredFocus +=
            throw new NotImplementedException();
        }
    }

    private void RedrawChild(UIElement child, RedrawElementEventArgs args)
    {
        throw new NotImplementedException();
    }

    private void RemoveChild(UIElement sender, RemoveElementEventsArgs args)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Abstract members

    public abstract int BufferWidth { get; }

    public abstract int BufferHeight { get; }

    protected abstract void ResolveChildSizeAndPosition(UIElement child,
        out int left, out int top, out int height, out int width);

    protected abstract void DrawChild(ChildInfo child);

    protected abstract void EraseChild(UIElement child);

    protected abstract void RunWithCancellation(CancellationToken cancellationToken);

    #endregion
    
    public static UIManager Initialize(UIManagerSettings settings)
    {
        throw new NotImplementedException();
        return Instance;
    }

    #region Constructors
    
    protected UIManager(UIManagerSettings settings)
    {
        Settings = settings;
    }

    #endregion
}