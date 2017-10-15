using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class YearTemplateSegment : TemplateSegment
    {
        public static YearTemplateSegment Instance { get; } = new YearTemplateSegment();
        private YearTemplateSegment() { }

        public override void AppendToTextEvaluation(DynamicTemplateValues currentValues, StringBuilder textBuilder, TemplateSegmentMatchResult previousMatchResult = null)
        {
            if (currentValues.CurrentYear < 1000 || currentValues.CurrentYear > 9999)
                throw new ArgumentOutOfRangeException(nameof(currentValues), currentValues.CurrentYear, "Current year must be between 1000 and 9999, inclusive.");

            textBuilder.Append(currentValues.CurrentYear);
        }

        public override void AppendToMatchRegex(StringBuilder regexBuilder)
        {
            regexBuilder.Append(@"\d{4}");
        }

        public override TemplateSegmentMatchResult GetMatchResult(DynamicTemplateValues currentValues, string matchText, int start, int length, ImmutableArray<Group> innerGroups)
        {
            var year = int.Parse(matchText.Substring(start, length));

            return new TemplateSegmentMatchResult(
                isInexact: false,
                errorMessages: currentValues.CurrentYear < year ? ImmutableArray.Create($"The year {year} is invalid. The current year is {currentValues.CurrentYear}.") : ImmutableArray<string>.Empty,
                updateMessages: year < currentValues.CurrentYear ? ImmutableArray.Create($"The current year is {currentValues.CurrentYear}.") : ImmutableArray<string>.Empty);
        }
    }
}
