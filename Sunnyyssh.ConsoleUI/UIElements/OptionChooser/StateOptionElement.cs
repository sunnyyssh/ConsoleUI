namespace Sunnyyssh.ConsoleUI;

public abstract class StateOptionElement : OptionElement
{
    private bool _isChosen;
    
    private bool _isFocused;
    public override bool IsChosen => _isChosen;

    public override bool IsFocused => _isFocused;

    protected override DrawState CreateDrawState(int width, int height)
    {
        var state = RequestState(_isChosen, _isFocused);
        return state;
    }

    public override void ChosenOn()
    {
        if (_isChosen)
        {
            return;
        }
        
        _isChosen = true;
        if (IsDrawn)
        {
            var state = RequestState(true, _isFocused);
            Redraw(state);
        }
    }
    
    public override void ChosenOff()
    {
        if (!_isChosen)
        {
            return;
        }
        
        _isChosen = false;
        if (IsDrawn)
        {
            var state = RequestState(false, _isFocused);
            Redraw(state);
        }
    }

    public override void FocusOn()
    {
        if (_isFocused)
        {
            return;
        }

        _isFocused = true;
        if (IsDrawn)
        {
            var state = RequestState(_isChosen, true);
            Redraw(state);
        }
    }

    public override void FocusOff()
    {
        if (!_isFocused)
        {
            return;
        }

        _isFocused = false;
        if (IsDrawn)
        {
            var state = RequestState(_isChosen, false);
            Redraw(state);
        }
    }

    protected abstract DrawState RequestState(bool isChosen, bool isFocused);

    protected StateOptionElement(int width, int height) : base(width, height)
    {
    }
}