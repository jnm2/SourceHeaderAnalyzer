using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class TextTemplateSegment : TemplateSegment
    {
        private readonly string text;

        public TextTemplateSegment(string text)
        {
            this.text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override void AppendToTextEvaluation(DynamicTemplateValues currentValues, StringBuilder textBuilder, TemplateSegmentMatchResult previousMatchResult = null)
        {
            textBuilder.Append(text);
        }

        public override void AppendToMatchRegex(StringBuilder regexBuilder)
        {
            var escaped = Regex.Escape(text);
            var detectCopyrightSymbolChanges = Regex.Replace(escaped, @"\(\s*c\s*\)|©", @"(?:\(\s*c\s*\)|©)");
            var detectWhitespaceChanges = Regex.Replace(detectCopyrightSymbolChanges, @"(?:\\[\srnt])+", @"\s*");

            regexBuilder.Append(detectWhitespaceChanges);
        }

        public override TemplateSegmentMatchResult GetMatchResult(DynamicTemplateValues currentValues, string matchText, int start, int length, ImmutableArray<Group> innerGroups)
        {
            return new TemplateSegmentMatchResult(
                isInexact: text.Length != length || string.Compare(text, 0, matchText, start, length, StringComparison.OrdinalIgnoreCase) != 0,
                errorMessages: ImmutableArray<string>.Empty,
                updateMessages: ImmutableArray<string>.Empty);
        }
    }
}
