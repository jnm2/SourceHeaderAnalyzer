using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class YearRangeTemplateSegment : TemplateSegment
    {
        private readonly int startYear;
        private readonly int currentYear;

        public YearRangeTemplateSegment(int startYear, int currentYear)
        {
            if (startYear < 1000 || startYear > 9999) throw new ArgumentOutOfRangeException(nameof(startYear), startYear, "Start year must be between 1000 and 9999, inclusive.");
            if (currentYear < startYear || currentYear > 9999) throw new ArgumentOutOfRangeException(nameof(currentYear), currentYear, "Current year must be between start year and 9999, inclusive.");

            this.startYear = startYear;
            this.currentYear = currentYear;
        }

        public override void AppendToTextEvaluation(StringBuilder textBuilder)
        {
            BuildText(textBuilder, startYear, currentYear);
        }

        public override void AppendToMatchRegex(StringBuilder regexBuilder)
        {
            regexBuilder.Append(@"(\d{4})(?:\s*[-–—]+\s*(\d{4}))?");
        }

        private static void BuildText(StringBuilder textBuilder, int startYear, int currentYear)
        {
            textBuilder.Append(startYear);

            if (startYear != currentYear)
                textBuilder.Append('–').Append(currentYear); // (en dash)
        }

        public override TemplateSegmentMatchResult GetMatchResult(string matchText, int start, int length, ImmutableArray<Group> innerGroups)
        {
            var matchStartYear = int.Parse(innerGroups[0].Value);
            var matchCurrentYear = innerGroups.Length == 2 ? int.Parse(innerGroups[1].Value) : matchStartYear;

            var errorMessages = ImmutableArray.CreateBuilder<string>();
            if (matchCurrentYear < matchStartYear)
                errorMessages.Add($"The start year ({matchStartYear}) must not be greater than the end year ({matchCurrentYear}).");
            if (currentYear < matchCurrentYear)
                errorMessages.Add($"The year {matchCurrentYear} is invalid. The current year is {currentYear}.");

            var exactText = new StringBuilder();
            BuildText(exactText, matchStartYear, matchCurrentYear);

            return new TemplateSegmentMatchResult(
                isInexact: exactText.Length != length || string.Compare(exactText.ToString(), 0, matchText, start, length, StringComparison.OrdinalIgnoreCase) != 0,
                errorMessages: errorMessages.ToImmutable(),
                updateMessages: matchCurrentYear < currentYear ? ImmutableArray.Create($"The current year is {currentYear}.") : ImmutableArray<string>.Empty);
        }
    }
}
