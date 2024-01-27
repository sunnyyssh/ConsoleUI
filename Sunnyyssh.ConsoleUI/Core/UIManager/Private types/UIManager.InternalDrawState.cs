// Tested type.

namespace Sunnyyssh.ConsoleUI;

partial class UIManager
{
    //TODO make it protected.
    public sealed class InternalDrawState
    {
        // TODO
        public PixelLine[] Lines { get; private init; }

        public InternalDrawState(PixelLine[] lines)
        {
            Lines = lines ?? throw new ArgumentNullException(nameof(lines));
        }

        public static InternalDrawState Combine(params InternalDrawState[] orderedDrawStates)
        {
            ArgumentNullException.ThrowIfNull(orderedDrawStates, nameof(orderedDrawStates));

            var lines = orderedDrawStates
                // making one sequence of lines from sequence of sequences of lines.
                .SelectMany(
                    state => state.Lines,
                    (_, line) => line)
                // Grouping lines by their Top value
                // and making one line from each group using overlapping
                .GroupBy(
                    line => line.Top,
                    line => line,
                    (_, lines) => PixelLine.Overlap(lines.ToArray()))
                .ToArray();

            InternalDrawState result = new InternalDrawState(lines);
            return result;
        }

        public static InternalDrawState Shift(int leftShift, int topShift, InternalDrawState drawState)
        {
            return new InternalDrawState(
                drawState.Lines
                .Select(l => 
                    new PixelLine(
                        l.Left + leftShift, 
                        l.Top + topShift, 
                        l.Pixels))
                .ToArray());
        }
    }
}