using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;

namespace SourceHeaderAnalyzer
{
    public sealed class SourceTextReader : TextReader
    {
        private readonly SourceText text;
        private int position;

        public SourceTextReader(SourceText text)
        {
            this.text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public override int Read()
        {
            return text.Length <= position ? -1 : text[position++];
        }

        public override int Peek()
        {
            return text.Length <= position ? -1 : text[position];
        }

        public override int Read(char[] buffer, int index, int count)
        {
            count = Math.Min(count, text.Length - position);
            text.CopyTo(position, buffer, index, count);
            position += count;
            return count;
        }

        public override Task<int> ReadAsync(char[] buffer, int index, int count)
        {
            return Task.FromResult(Read(buffer, index, count));
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return Read(buffer, index, count);
        }

        public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
        {
            return Task.FromResult(ReadBlock(buffer, index, count));
        }

        public override string ReadLine()
        {
            var line = text.Lines.GetLineFromPosition(position);
            var lineText = text.ToString(TextSpan.FromBounds(position, line.End));
            position = line.EndIncludingLineBreak;
            return lineText;
        }

        public override Task<string> ReadLineAsync()
        {
            return Task.FromResult(ReadLine());
        }

        public override string ReadToEnd()
        {
            var rest = text.ToString(TextSpan.FromBounds(position, text.Length));
            position = text.Length;
            return rest;
        }

        public override Task<string> ReadToEndAsync()
        {
            return Task.FromResult(ReadToEnd());
        }
    }
}
