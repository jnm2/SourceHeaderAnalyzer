using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class NameTemplateSegment : TemplateSegment
    {
        public string DefaultName { get; }
        private const string Placeholder = "{Name}";

        public NameTemplateSegment(string defaultName)
        {
            defaultName = defaultName?.Trim();
            if (string.IsNullOrEmpty(defaultName)) defaultName = Placeholder;
            DefaultName = defaultName;
        }

        public override void AppendToTextEvaluation(DynamicTemplateValues currentValues, StringBuilder textBuilder, TemplateSegmentMatchResult previousMatchResult = null)
        {
            textBuilder.Append((previousMatchResult as MatchResult)?.TrimmedName ?? DefaultName);
        }

        public override void AppendToMatchRegex(StringBuilder regexBuilder)
        {
            regexBuilder.Append(".*?");
        }

        public override TemplateSegmentMatchResult GetMatchResult(DynamicTemplateValues currentValues, string matchText, int start, int length, ImmutableArray<Group> innerGroups)
        {
            var trimmedMatch = matchText.Substring(start, length).Trim();

            return new MatchResult(
                isInexact: trimmedMatch.Length != length,
                errorMessages: trimmedMatch == string.Empty || trimmedMatch == Placeholder ? ImmutableArray.Create("Name must not be blank.") : ImmutableArray<string>.Empty,
                updateMessages: ImmutableArray<string>.Empty,
                trimmedName: trimmedMatch);
        }

        private sealed class MatchResult : TemplateSegmentMatchResult
        {
            public string TrimmedName { get; }

            public MatchResult(bool isInexact, ImmutableArray<string> errorMessages, ImmutableArray<string> updateMessages, string trimmedName)
                : base(isInexact, errorMessages, updateMessages)
            {
                TrimmedName = trimmedName;
            }
        }
    }
}
