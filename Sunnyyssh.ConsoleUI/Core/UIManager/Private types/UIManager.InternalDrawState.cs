namespace Sunnyyssh.ConsoleUI;

partial class UIManager
{
    protected class InternalDrawState
    {
        // TODO

        public static InternalDrawState Combine(params InternalDrawState[] drawStates)
        {
            return Combine((IEnumerable<InternalDrawState>)drawStates);
        }
        
        public static InternalDrawState Combine(IEnumerable<InternalDrawState> drawStates)
        {
            throw new NotImplementedException("Stupid ass damn ass");
        }
    }
}