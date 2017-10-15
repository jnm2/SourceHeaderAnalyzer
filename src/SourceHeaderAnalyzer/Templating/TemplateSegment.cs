using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public abstract class TemplateSegment
    {
        public abstract void AppendToTextEvaluation(DynamicTemplateValues currentValues, StringBuilder textBuilder);
        public abstract void AppendToMatchRegex(StringBuilder regexBuilder);
        public abstract TemplateSegmentMatchResult GetMatchResult(DynamicTemplateValues currentValues, string matchText, int start, int length, ImmutableArray<Group> innerGroups);
    }
}
