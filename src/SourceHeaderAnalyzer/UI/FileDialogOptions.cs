using System.Collections.Generic;

namespace SourceHeaderAnalyzer.UI
{
    public sealed class FileDialogOptions
    {
        public string WindowTitle { get; set; }
        public string InitialFileName { get; set; }
        public string InitialDirectory { get; set; }
        public IReadOnlyList<FileDialogFilter> Filters { get; set; }
        public int InitialFilterIndex { get; set; }
    }
}
