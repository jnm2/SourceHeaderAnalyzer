using System;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceHeaderAnalyzer.Templating
{
    public sealed class TextTemplateSegment : TemplateSegment
    {
        public string Text { get; }

        public TextTemplateSegment(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override void AppendToTextEvaluation(DynamicTemplateValues currentValues, StringBuilder textBuilder, TemplateSegmentMatchResult previousMatchResult = null)
        {
            textBuilder.Append(Text);
        }

        public override void AppendToMatchRegex(StringBuilder regexBuilder)
        {
            var endingWhitespace = Regex.Match(Text, @"\s+\Z");

            var textWithoutEndingWhitespace = Text.Substring(0, Text.Length - endingWhitespace.Length);
            var escaped = Regex.Escape(textWithoutEndingWhitespace);
            var detectCopyrightSymbolChanges = Regex.Replace(escaped, @"\(\s*c\s*\)|©", @"(?:\(\s*c\s*\)|©)");
            var detectWhitespaceChanges = Regex.Replace(detectCopyrightSymbolChanges, @"(?:\\[\srnt])+", @"\s*");

            regexBuilder.Append(detectWhitespaceChanges);

            var detectWhitespaceChangesExceptNewLines = Regex.Replace(Regex.Escape(endingWhitespace.Value), @"(?:\\[\st])*(?=\\r|(?<!\\r)\\n)", @"[^\S\r\n]*");
            regexBuilder.Append(detectWhitespaceChangesExceptNewLines);
        }

        public override TemplateSegmentMatchResult GetMatchResult(DynamicTemplateValues currentValues, string matchText, int start, int length, ImmutableArray<Group> innerGroups)
        {
            return new TemplateSegmentMatchResult(
                isInexact: Text.Length != length || string.Compare(Text, 0, matchText, start, length, StringComparison.OrdinalIgnoreCase) != 0,
                errorMessages: ImmutableArray<string>.Empty,
                updateMessages: ImmutableArray<string>.Empty);
        }
    }
}
