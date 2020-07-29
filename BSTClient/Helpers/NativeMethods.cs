using System;
using System.Runtime.InteropServices;

namespace BSTClient.Helpers
{
    class NativeMethods
    {
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        private const int SHGFI_TYPENAME = 0x400;
        private const int SHGFI_USEFILEATTRIBUTES = 0x10;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            int dwFileAttributes,
            ref SHFILEINFO shinfo,
            uint cbfileInfo,
            int uFlags);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero;
                iIcon = 0;
                dwAttributes = 0;
                szDisplayName = "";
                szTypeName = "";
            }

            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };


        public static string GetShellFileType(string fileName)
        {
            var shinfo = new SHFILEINFO(true);
            const int flags = SHGFI_TYPENAME | SHGFI_USEFILEATTRIBUTES;

            if (SHGetFileInfo(fileName, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags) ==
                IntPtr.Zero)
            {
                return "File";
            }

            return shinfo.szTypeName;
        }
    }
}