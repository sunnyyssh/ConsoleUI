namespace Sunnyyssh.ConsoleUI;

partial class UIManager
{
    public class InternalDrawState
    {
        // TODO

        public static InternalDrawState Combine(params InternalDrawState[] drawStates)
        {
            return Combine((IEnumerable<InternalDrawState>)drawStates);
        }
        
        public static InternalDrawState Combine(IEnumerable<InternalDrawState> drawStates)
        {
            return new InternalDrawState();
            throw new NotImplementedException("Stupid ass damn ass");
        }
    }
}