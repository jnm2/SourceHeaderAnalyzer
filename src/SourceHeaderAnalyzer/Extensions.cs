using System;
using System.Text;
using System.Threading.Tasks;

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

        public static T AssertCompletedSynchronously<T>(this Task<T> task)
        {
            var awaiter = task.GetAwaiter();
            if (!awaiter.IsCompleted) throw new InvalidOperationException("The task did not completed synchronously.");
            return awaiter.GetResult();
        }
    }
}
