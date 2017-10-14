using System.Collections.Immutable;

namespace SourceHeaderAnalyzer.Templating
{
    public struct MatchResult
    {
        public MatchResult(int start, int length, bool isInexact, ImmutableArray<string> errorMessages, ImmutableArray<string> updateMessages)
        {
            Start = start;
            Length = length;
            IsInexact = isInexact;
            ErrorMessages = errorMessages;
            UpdateMessages = updateMessages;
        }

        public int Start { get; }
        public int Length { get; }
        public bool IsInexact { get; }
        public ImmutableArray<string> ErrorMessages { get; }
        public ImmutableArray<string> UpdateMessages { get; }
    }
}
