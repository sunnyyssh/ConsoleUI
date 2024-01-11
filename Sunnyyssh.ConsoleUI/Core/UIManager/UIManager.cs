using System.Diagnostics;

namespace Sunnyyssh.ConsoleUI;

public abstract class UIManager
{
    private static UIManager? _instance;

    public static UIManager? Instance
    {
        [DebuggerStepThrough] get => _instance;
        [DebuggerStepThrough] private set => _instance = value ?? throw new ArgumentNullException();
    }

    public static bool IsInitialized => _instance is not null;

    public bool IsRunning { get; protected set; }

    public int Width => Console.BufferWidth;

    public int Height => Console.BufferHeight;

    public abstract int BufferWidth { get; }

    public abstract int BufferHeight { get; }

    protected List<UIElement> ChildrenList { get; private init; } = new List<UIElement>();
    
    public UIElement[] Children => ChildrenList.ToArray();

    public static UIManager Initialize(UIManagerSettings settings)
    {
        throw new NotImplementedException();
        return Instance;
    }

    public abstract void AddChild(UIElement child);

    public abstract void RemoveChild(UIElement child);

    public abstract void Run();

    public abstract void Exit();
}