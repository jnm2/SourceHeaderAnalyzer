using System;

namespace SourceHeaderAnalyzer.UI
{
    public sealed class FileDialogFilter
    {
        public string Name { get; }
        public string Filter { get; }

        public FileDialogFilter(string name, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                throw new ArgumentException("Filter must not be blank.", nameof(filter));

            Name = name ?? filter;
            Filter = filter;
        }
    }
}
