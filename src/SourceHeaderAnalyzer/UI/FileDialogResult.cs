namespace SourceHeaderAnalyzer.UI
{
    public sealed class FileDialogResult
    {
        public FileDialogResult(string fileName, int selectedFilterIndex)
        {
            FileName = fileName;
            SelectedFilterIndex = selectedFilterIndex;
        }

        public string FileName { get; }
        public int SelectedFilterIndex { get; }
    }
}
