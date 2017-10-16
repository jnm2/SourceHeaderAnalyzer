using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class HeaderTemplate
    {
        public ImmutableArray<TemplateSegment> Segments { get; }

        private readonly Lazy<Regex> regex;

        public HeaderTemplate(ImmutableArray<TemplateSegment> segments)
        {
            Segments = segments;
            regex = new Lazy<Regex>(CreateRegex);
        }

        public void Evaluate(DynamicTemplateValues currentValues, StringBuilder textBuilder, string previousText, out int previousMatchStart, out int previousMatchLength)
        {
            if (previousText != null && TryMatch(previousText, currentValues, out previousMatchStart, out previousMatchLength, out var segmentResults))
            {
                for (var i = 0; i < Segments.Length; i++)
                    Segments[i].AppendToTextEvaluation(currentValues, textBuilder, segmentResults[i]);
            }
            else
            {
                previousMatchStart = -1;
                previousMatchLength = -1;
                Evaluate(currentValues, textBuilder);
            }
        }

        public void Evaluate(DynamicTemplateValues currentValues, StringBuilder textBuilder)
        {
            foreach (var segment in Segments)
                segment.AppendToTextEvaluation(currentValues, textBuilder);
        }

        private Regex CreateRegex()
        {
            var regexBuilder = new StringBuilder();

            for (var i = 0; i < Segments.Length; i++)
            {
                regexBuilder.Append("(?<HeaderTemplate_segment").Append(i).Append('>');
                Segments[i].AppendToMatchRegex(regexBuilder);
                regexBuilder.Append(')');
            }

            return new Regex(regexBuilder.ToString(), RegexOptions.IgnoreCase);
        }

        private bool TryMatch(string text, DynamicTemplateValues currentValues, out int start, out int length, out ImmutableArray<TemplateSegmentMatchResult> segmentResults)
        {
            var match = regex.Value.Match(text);
            if (!match.Success)
            {
                start = default;
                length = default;
                segmentResults = default;
                return false;
            }

            var segmentResultsBuilder = ImmutableArray.CreateBuilder<TemplateSegmentMatchResult>(Segments.Length);

            length = match.Length;

            for (var i = 0; i < Segments.Length; i++)
            {
                var segmentGroup = match.Groups["HeaderTemplate_segment" + i];

                var innerGroups = ImmutableArray.CreateBuilder<Group>();
                foreach (Group group in match.Groups)
                {
                    if (group.Success
                        && group != segmentGroup
                        && group.Index >= segmentGroup.Index
                        && group.Index + group.Length <= segmentGroup.Index + segmentGroup.Length)
                    {
                        innerGroups.Add(group);
                    }
                }

                var segmentLength = segmentGroup.Length;

                if (i == Segments.Length - 1)
                {
                    var lengthMeasure = new StringBuilder();
                    Segments[i].AppendToTextEvaluation(currentValues, lengthMeasure);
                    var greed = segmentGroup.Length - lengthMeasure.Length;
                    if (0 < greed && regex.Value.IsMatch(text.Substring(0, segmentGroup.Index + lengthMeasure.Length)))
                    {
                        length -= greed;
                        segmentLength -= greed;
                    }
                }

                segmentResultsBuilder.Add(Segments[i].GetMatchResult(
                    currentValues,
                    text,
                    segmentGroup.Index,
                    segmentLength,
                    innerGroups.ToImmutable()));
            }

            start = match.Index;
            segmentResults = segmentResultsBuilder.ToImmutable();
            return true;
        }

        public bool TryMatch(string text, DynamicTemplateValues currentValues, out MatchResult result)
        {
            if (!TryMatch(text, currentValues, out var start, out var length, out var segmentResults))
            {
                result = default;
                return false;
            }

            var isInexact = false;
            var errorMessages = ImmutableArray.CreateBuilder<string>();
            var updateMessages = ImmutableArray.CreateBuilder<string>();

            foreach (var segmentResult in segmentResults)
            {
                isInexact |= segmentResult.IsInexact;
                errorMessages.AddRange(segmentResult.ErrorMessages);
                updateMessages.AddRange(segmentResult.UpdateMessages);
            }

            result = new MatchResult(
                start,
                length,
                isInexact,
                errorMessages.ToImmutable(),
                updateMessages.ToImmutable());

            return true;
        }
    }
}
