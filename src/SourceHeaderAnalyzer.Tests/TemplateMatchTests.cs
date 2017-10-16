using System.Collections.Immutable;
using NUnit.Framework;
using SourceHeaderAnalyzer.Templating;

namespace SourceHeaderAnalyzer.Tests
{
    public static class TemplateMatchTests
    {
        [Test]
        public static void Last_segment_is_not_greedy()
        {
            var template = new HeaderTemplate(ImmutableArray.Create<TemplateSegment>(
                new TextTemplateSegment("a ")));

            Assert.That(template.TryMatch("a  ", default, out var result));

            Assert.That(result, Has.Property("Length").EqualTo(2));
        }

        [Test]
        public static void Last_segment_is_not_inexact()
        {
            var template = new HeaderTemplate(ImmutableArray.Create<TemplateSegment>(
                new TextTemplateSegment("a ")));

            Assert.That(template.TryMatch("a  ", default, out var result));

            Assert.That(result, Has.Property("IsInexact").EqualTo(false));
        }

        [Test]
        public static void Only_correct_greed_if_exact()
        {
            var template = new HeaderTemplate(ImmutableArray.Create<TemplateSegment>(
                new TextTemplateSegment("a a ")));

            Assert.That(template.TryMatch("a  a ", default, out var result));

            Assert.That(result, Has.Property("Length").EqualTo(5));
        }

        [Test]
        public static void Allow_whitespace_greed_within_ending_lines()
        {
            var template = new HeaderTemplate(ImmutableArray.Create<TemplateSegment>(
                new TextTemplateSegment("a\r\n\r\n")));

            Assert.That(template.TryMatch("a \r\n \r\n \r\n", default, out var result));

            Assert.That(result, Has.Property("Length").EqualTo(7));
        }
    }
}
