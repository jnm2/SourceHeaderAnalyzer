using System;
using System.Text;

namespace SourceHeaderAnalyzer
{
    internal static class Extensions
    {
        public static bool EndsWithOrdinal(this StringBuilder builder, string value)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(value)) return true;

            var offset = builder.Length - value.Length;
            if (offset < 0) return false;

            for (var i = 0; i < value.Length; i++)
                if (builder[offset + i] != value[i]) return false;

            return true;
        }
    }
}
