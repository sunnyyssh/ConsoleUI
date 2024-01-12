namespace Sunnyyssh.ConsoleUI;

public sealed class UIManagerSettings
{
    private readonly Color _background = Color.White;

    private readonly Color _foreground = Color.Black;

    private readonly FocusFlowMode _focusFlowMode;
    
    public FocusFlowMode FocusFlowMode
    {
        get => _focusFlowMode;
        init
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentException("Enum value is not defined.", nameof(value));
        }
    }

    public Color Background
    {
        get => _background;
        init
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentException("Enum value is not defined.", nameof(value));
            if (value != Color.Transparent)
            {
                _background = value;
            }
        }
    }
    
    public Color Foreground
    {
        get => _foreground;
        init
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentException("Enum value is not defined.", nameof(value));
            if (value != Color.Transparent)
            {
                _foreground = value;
            }
        }
    }

    public UIManagerSettings()
    {
        
    }
}