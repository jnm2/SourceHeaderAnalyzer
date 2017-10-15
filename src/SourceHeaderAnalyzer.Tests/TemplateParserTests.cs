using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using NUnit.Framework;
using SourceHeaderAnalyzer.Templating;

namespace SourceHeaderAnalyzer.Tests
{
    public static class TemplateParserTests
    {
        private static ImmutableArray<TemplateSegment> Parse(string template)
        {
            return TemplateParser.Parse(new StringReader(template)).Item1.Segments;
        }

        private static void AssertSingleText(string template, string expected)
        {
            var result = Parse(template);
            Assert.That(result, Has.One.Items);
            Assert.That(result[0], Is.TypeOf<TextTemplateSegment>());
            Assert.That(((TextTemplateSegment)result[0]).Text, Is.EqualTo(expected).Using((IComparer<string>)StringComparer.Ordinal));
        }

        [TestCase(" ")]
        [TestCase("a\r\nb")]
        public static void New_line_is_added_if_current_line_is_not_empty(string text)
        {
            AssertSingleText(text, text + "\r\n");
        }

        [TestCase("\r\n")]
        [TestCase("a\r\nb\r\n")]
        public static void New_line_is_not_added_if_current_line_is_empty(string text)
        {
            AssertSingleText(text, text);
        }
    }
}
