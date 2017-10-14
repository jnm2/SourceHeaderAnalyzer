using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class YearTemplateSegment : TemplateSegment
    {
        private readonly int currentYear;

        public YearTemplateSegment(int currentYear)
        {
            if (currentYear < 1000 || currentYear > 9999) throw new ArgumentOutOfRangeException(nameof(currentYear), currentYear, "Year must be between 1000 and 9999, inclusive.");
            this.currentYear = currentYear;
        }

        public override void AppendToTextEvaluation(StringBuilder textBuilder)
        {
            textBuilder.Append(currentYear);
        }

        public override void AppendToMatchRegex(StringBuilder regexBuilder)
        {
            regexBuilder.Append(@"\d{4}");
        }

        public override TemplateSegmentMatchResult GetMatchResult(string matchText, int start, int length, ImmutableArray<Group> innerGroups)
        {
            var year = int.Parse(matchText.Substring(start, length));

            return new TemplateSegmentMatchResult(
                isInexact: false,
                errorMessages: currentYear < year ? ImmutableArray.Create($"The year {year} is invalid. The current year is {currentYear}.") : ImmutableArray<string>.Empty,
                updateMessages: year < currentYear ? ImmutableArray.Create($"The current year is {currentYear}.") : ImmutableArray<string>.Empty);
        }
    }
}
