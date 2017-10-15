using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class HeaderTemplate
    {
        private readonly ImmutableArray<TemplateSegment> segments;
        private readonly Lazy<Regex> regex;

        public HeaderTemplate(ImmutableArray<TemplateSegment> segments)
        {
            this.segments = segments;
            regex = new Lazy<Regex>(CreateRegex);
        }

        private Regex CreateRegex()
        {
            var regexBuilder = new StringBuilder();

            for (var i = 0; i < segments.Length; i++)
            {
                regexBuilder.Append("(?<HeaderTemplate_segment").Append(i).Append('>');
                segments[i].AppendToMatchRegex(regexBuilder);
                regexBuilder.Append(')');
            }

            return new Regex(regexBuilder.ToString(), RegexOptions.IgnoreCase);
        }

        public bool TryMatch(string text, DynamicTemplateValues currentValues, out MatchResult result)
        {
            var match = regex.Value.Match(text);
            if (!match.Success)
            {
                result = default;
                return false;
            }

            var errorMessages = ImmutableArray.CreateBuilder<string>();
            var updateMessages = ImmutableArray.CreateBuilder<string>();
            var isInexact = false;

            for (var i = 0; i < segments.Length; i++)
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

                var segmentResult = segments[i].GetMatchResult(currentValues, text, segmentGroup.Index, segmentGroup.Length, innerGroups.ToImmutable());

                isInexact |= segmentResult.IsInexact;
                errorMessages.AddRange(segmentResult.ErrorMessages);
                updateMessages.AddRange(segmentResult.UpdateMessages);
            }

            result = new MatchResult(
                match.Index,
                match.Length,
                isInexact,
                errorMessages.ToImmutable(),
                updateMessages.ToImmutable());

            return true;
        }
    }
}
