using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Inkybot
{
    public class WindowHelpers
    {
        public static IntPtr DockProcess(Process process, Panel destination, ref IntPtr hWndDocked) {
            if (hWndDocked != IntPtr.Zero)
                return IntPtr.Zero;

            while (hWndDocked == IntPtr.Zero) {
                process.WaitForInputIdle(1000);
                process.Refresh();
                if (process.HasExited)
                    return IntPtr.Zero;
                hWndDocked = process.MainWindowHandle;
            }

            var oldParentHandle = Win32.SetParent(hWndDocked, destination.Handle);

            var docked = hWndDocked;
            EventHandler moveEventHandler = (sender, e) =>
                Win32.MoveWindow(docked, 0, 0, destination.Width, destination.Height, true);
            destination.SizeChanged += moveEventHandler;
            moveEventHandler(new object(), new EventArgs());

            return oldParentHandle;
        }

        public static void UndockProcess(IntPtr handle, IntPtr handleDestination) {
            Win32.SetParent(handle, handleDestination);
        }


        public static void RemoveWindowBorders(IntPtr window) {
            var style = Win32.GetWindowLong(window, Win32.GWL_STYLE);
            Win32.SetWindowLong(window, Win32.GWL_STYLE, style & ~Win32.WS_CAPTION);
        }
        
        
        [DllImport("shell32.dll", ExactSpelling = true)]
        public static extern int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
            uint dwFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        public interface IShellLinkW
        {
            [PreserveSig]
            int GetPath(StringBuilder pszFile, int cch, [In, Out] ref WIN32_FIND_DATAW pfd, uint fFlags);

            [PreserveSig]
            int GetIDList([Out] out IntPtr ppidl);

            [PreserveSig]
            int SetIDList([In] ref IntPtr pidl);

            [PreserveSig]
            int GetDescription(StringBuilder pszName, int cch);

            [PreserveSig]
            int SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            [PreserveSig]
            int GetWorkingDirectory(StringBuilder pszDir, int cch);

            [PreserveSig]
            int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            [PreserveSig]
            int GetArguments(StringBuilder pszArgs, int cch);

            [PreserveSig]
            int SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            [PreserveSig]
            int GetHotkey([Out] out ushort pwHotkey);

            [PreserveSig]
            int SetHotkey(ushort wHotkey);

            [PreserveSig]
            int GetShowCmd([Out] out int piShowCmd);

            [PreserveSig]
            int SetShowCmd(int iShowCmd);

            [PreserveSig]
            int GetIconLocation(StringBuilder pszIconPath, int cch, [Out] out int piIcon);

            [PreserveSig]
            int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            [PreserveSig]
            int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

            [PreserveSig]
            int Resolve(IntPtr hwnd, uint fFlags);

            [PreserveSig]
            int SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), BestFitMapping(false)]
        public struct WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        public static void OpenFolderAndSelectFiles(string folder, params string[] filesToSelect)
        {
            IntPtr dir = ILCreateFromPath(folder);

            var filesToSelectIntPtrs = new IntPtr[filesToSelect.Length];
            for (int i = 0; i < filesToSelect.Length; i++)
            {
                filesToSelectIntPtrs[i] = ILCreateFromPath(filesToSelect[i]);
            }

            SHOpenFolderAndSelectItems(dir, (uint) filesToSelect.Length, filesToSelectIntPtrs, 0);
            ReleaseComObject(dir);
            ReleaseComObject(filesToSelectIntPtrs);
        }

        private static void ReleaseComObject(params object[] comObjs)
        {
            foreach (object obj in comObjs)
            {
                if (obj != null && Marshal.IsComObject(obj))
                    Marshal.ReleaseComObject(obj);
            }
        }
    }
}
