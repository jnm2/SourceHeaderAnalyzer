using System.Collections.Immutable;

namespace SourceHeaderAnalyzer.Templating
{
    public class TemplateSegmentMatchResult
    {
        public bool IsInexact { get; }
        public ImmutableArray<string> ErrorMessages { get; }
        public ImmutableArray<string> UpdateMessages { get; }

        public TemplateSegmentMatchResult(bool isInexact, ImmutableArray<string> errorMessages, ImmutableArray<string> updateMessages)
        {
            IsInexact = isInexact;
            ErrorMessages = errorMessages;
            UpdateMessages = updateMessages;
        }
    }
}
