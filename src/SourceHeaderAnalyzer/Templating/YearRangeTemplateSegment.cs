using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class YearRangeTemplateSegment : TemplateSegment
    {
        private readonly int startYear;

        public YearRangeTemplateSegment(int startYear, DynamicTemplateValues currentValuesForValidation)
        {
            if (startYear < 1000 || startYear > currentValuesForValidation.CurrentYear)
                throw new ArgumentOutOfRangeException(nameof(startYear), startYear, "Start year must be between 1000 and the current year, inclusive.");

            this.startYear = startYear;
        }

        public override void AppendToTextEvaluation(DynamicTemplateValues currentValues, StringBuilder textBuilder)
        {
            BuildText(textBuilder, startYear, currentValues.CurrentYear);
        }

        public override void AppendToMatchRegex(StringBuilder regexBuilder)
        {
            regexBuilder.Append(@"(\d{4})(?:\s*[-–—]+\s*(\d{4}))?");
        }

        private static void BuildText(StringBuilder textBuilder, int startYear, int currentYear)
        {
            textBuilder.Append(startYear);

            if (startYear != currentYear)
            {
                if (currentYear < 1000 || currentYear > 9999)
                    throw new ArgumentOutOfRangeException(nameof(currentYear), currentYear, "Current year must be between 1000 and 9999, inclusive.");

                textBuilder.Append('–').Append(currentYear); // (en dash)
            }
        }

        public override TemplateSegmentMatchResult GetMatchResult(DynamicTemplateValues currentValues, string matchText, int start, int length, ImmutableArray<Group> innerGroups)
        {
            var matchStartYear = int.Parse(innerGroups[0].Value);
            var matchCurrentYear = innerGroups.Length == 2 ? int.Parse(innerGroups[1].Value) : matchStartYear;

            var errorMessages = ImmutableArray.CreateBuilder<string>();
            if (matchCurrentYear < matchStartYear)
                errorMessages.Add($"The end year ({matchCurrentYear}) must be greater than or equal to the start year ({matchStartYear}).");

            if (currentValues.CurrentYear < matchCurrentYear)
                errorMessages.Add($"The year {matchCurrentYear} is invalid. The current year is {currentValues.CurrentYear}.");

            var exactText = new StringBuilder();
            BuildText(exactText, matchStartYear, matchCurrentYear);

            return new TemplateSegmentMatchResult(
                isInexact: exactText.Length != length || string.Compare(exactText.ToString(), 0, matchText, start, length, StringComparison.OrdinalIgnoreCase) != 0,
                errorMessages: errorMessages.ToImmutable(),
                updateMessages: matchCurrentYear < currentValues.CurrentYear ? ImmutableArray.Create($"The current year is {currentValues.CurrentYear}.") : ImmutableArray<string>.Empty);
        }
    }
}
