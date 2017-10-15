using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace SourceHeaderAnalyzer.UI
{
    partial class UserInterface
    {
        private sealed unsafe class WindowsUserInterface : UserInterface
        {
            private static readonly WindowsUserInterface Instance = new WindowsUserInterface();
            private WindowsUserInterface() { }

            public new static WindowsUserInterface TryGet()
            {
                try
                {
                    CommDlgExtendedError();
                }
                catch (DllNotFoundException)
                {
                    return null;
                }
                return Instance;
            }

            public override FileDialogResult SaveFile(FileDialogOptions options)
            {
                var filenameBuffer = new char[1024];

                fixed (char* filenamePointer = filenameBuffer)
                fixed (char* title = options.WindowTitle)
                fixed (char* initialDirectory = options.InitialDirectory)
                {
                    if (!string.IsNullOrWhiteSpace(options.InitialFileName))
                        options.InitialFileName.CopyTo(0, filenameBuffer, 0, options.InitialFileName.Length);

                    var nativeOptions = new OPENFILENAME(filenamePointer, filenameBuffer.Length)
                    {
                        lpstrTitle = title,
                        lpstrInitialDir = initialDirectory
                    };

                    char[] filterBuffer;
                    var neededFilterBufferSize = options.Filters.Sum(_ => (_.Name?.Length ?? 0) + _.Filter.Length + 2) + 1;
                    if (neededFilterBufferSize != 1)
                    {
                        filterBuffer = new char[neededFilterBufferSize];
                        var i = 0;
                        foreach (var filter in options.Filters)
                        {
                            filter.Name.CopyTo(0, filterBuffer, i, filter.Name.Length);
                            i += filter.Name.Length + 1;
                            filter.Filter.CopyTo(0, filterBuffer, i, filter.Filter.Length);
                            i += filter.Filter.Length + 1;
                        }

                        nativeOptions.nFilterIndex = options.InitialFilterIndex + 1;
                    }
                    else filterBuffer = null;

                    fixed (char* filterBufferPointer = filterBuffer)
                    {
                        nativeOptions.lpstrFilter = filterBufferPointer;

                        if (!GetSaveFileName(ref nativeOptions))
                        {
                            if (CommDlgExtendedError() == CDERR.UserCancelled)
                                return null;

                            throw new Exception("GetSaveFileName error");
                        }
                    }

                    var endIndex = Array.IndexOf(filenameBuffer, '\0');
                    return endIndex == 0 ? null : new FileDialogResult(new string(filenameBuffer, 0, endIndex), nativeOptions.nFilterIndex - 1);
                }
            }


            // ReSharper disable InconsistentNaming
            // ReSharper disable NotAccessedField.Local
            #pragma warning disable IDE1006 // Naming Styles
            #pragma warning disable 169
            #pragma warning disable 414
            #pragma warning disable 649

            [DllImport("comdlg32.dll", CharSet = CharSet.Unicode)]
            private static extern bool GetSaveFileName(ref OPENFILENAME lpofn);

            [DllImport("comdlg32.dll")]
            private static extern CDERR CommDlgExtendedError();

            private enum CDERR : uint
            {
                UserCancelled = 0
            }

            private struct OPENFILENAME
            {
                private readonly int lStructSize;
                private readonly IntPtr hwndOwner;
                private readonly IntPtr hInstance;
                public char* lpstrFilter;
                private readonly char* lpstrCustomFilter;
                private readonly int nMaxCustomFilter;
                public int nFilterIndex;
                private readonly char* lpstrFile;
                private readonly int nMaxFile;
                private readonly char* lpstrFileTitle;
                private readonly int nMaxFileTitle;
                public char* lpstrInitialDir;
                public char* lpstrTitle;
                private readonly uint Flags;
                private readonly ushort nFileOffset;
                private readonly ushort nFileExtension;
                private readonly char* lpstrDefExt;
                private readonly IntPtr lCustData;
                private readonly IntPtr lpfHook;
                private readonly IntPtr lpTemplateName;

                public OPENFILENAME(char* lpstrFile, int nMaxFile)
                {
                    this = default;
                    lStructSize = Marshal.SizeOf<OPENFILENAME>();
                    this.lpstrFile = lpstrFile;
                    this.nMaxFile = nMaxFile;
                }
            }
        }
    }
}
