using System;
using System.Runtime.InteropServices;

namespace BSTClient.Shared.Credentials.Natives
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct CredentialUIInfo
    {
        public int cbSize;
        public IntPtr hwndParent;
        public string? pszMessageText;
        public string? pszCaptionText;
        public IntPtr hbmBanner;
    }
}
