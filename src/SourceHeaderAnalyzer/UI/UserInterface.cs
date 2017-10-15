namespace SourceHeaderAnalyzer.UI
{
    public abstract partial class UserInterface
    {
        public abstract FileDialogResult SaveFile(FileDialogOptions options);

        public static UserInterface TryGet() => WindowsUserInterface.TryGet();
    }
}
