namespace Sunnyyssh.ConsoleUI;

public class UpdatedEventArgs : EventArgs
{
    public string PropertyName { get; }

    public UpdatedEventArgs(string propertyName)
    {
        PropertyName = propertyName;
    }
}

public delegate void UpdatedEventHandler<in TValue>(IObservable<TValue> sender, UpdatedEventArgs args);

public interface IObservable<out TValue>
{
    TValue? Value { get; }

    event UpdatedEventHandler<TValue> Updated;
}