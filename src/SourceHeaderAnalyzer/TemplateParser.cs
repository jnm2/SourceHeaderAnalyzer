using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using SourceHeaderAnalyzer.Templating;

namespace SourceHeaderAnalyzer
{
    public static class TemplateParser
    {
        public static OneOf<HeaderTemplate, string> Parse(TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var segments = ImmutableArray.CreateBuilder<TemplateSegment>();
            var buffer = new StringBuilder();

            for (;;)
            {
                var c = reader.Read();
                switch (c)
                {
                    case -1:
                        if (buffer.Length != 0)
                        {
                            segments.Add(new TextTemplateSegment(buffer.ToString()));
                            buffer.Clear();
                        }
                        return new HeaderTemplate(segments.ToImmutable());

                    case '{':
                        switch (reader.Peek())
                        {
                            case -1:
                                return "Unescaped '{' without closing '}'";

                            case '{':
                                buffer.Append('{');
                                break;

                            default:
                                if (buffer.Length != 0)
                                {
                                    segments.Add(new TextTemplateSegment(buffer.ToString()));
                                    buffer.Clear();
                                }
                                var result = ParseSpecialSegment(reader, buffer);
                                if (result.TryGetItem1(out var segment))
                                    segments.Add(segment);
                                else if (result.TryGetItem2(out var errorMessage))
                                    return errorMessage;
                                break;
                        }
                        break;

                    case '}':
                        if (reader.Read() != '}')
                            return "Unescaped '}' without opening '{'";
                        buffer.Append('}');
                        break;

                    default:
                        buffer.Append((char)c);
                        break;
                }
            }
        }

        /// <param name="reader">The current reader, positioned after the <c>{</c> character.</param>
        /// <param name="buffer">Must be empty and will be returned empty unless there is an exception.</param>
        private static OneOf<TemplateSegment, string> ParseSpecialSegment(TextReader reader, StringBuilder buffer)
        {
            for (;;)
            {
                var c = reader.Read();
                switch (c)
                {
                    case -1:
                        return "Unescaped '{' without closing '}'";

                    case '}':
                        var name = buffer.ToString();
                        buffer.Clear();

                        if (name.Equals("Year", StringComparison.OrdinalIgnoreCase))
                        {
                            return YearTemplateSegment.Instance;
                        }
                        else if (name.Equals("YearRange", StringComparison.OrdinalIgnoreCase))
                        {
                            return YearRangeTemplateSegment.Instance;
                        }

                        return $"Unrecognized special segment '{name}'";

                    default:
                        buffer.Append((char)c);
                        break;
                }
            }
        }
    }
}
