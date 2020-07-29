using Microsoft.Win32;
using System;
using System.IO;

namespace BSTClient.Helpers
{
    public class FileNameExtension
    {
        private static string _staticFileDesc;
        private static string _staticFolderDesc;

        public static string GetDescription(string ext)
        {
            if (ext.StartsWith(".") && ext.Length > 1) ext = ext.Substring(1);

            var retVal = ReadDefaultValue(ext + "file");
            if (!string.IsNullOrEmpty(retVal)) return retVal;


            using (var key = Registry.ClassesRoot.OpenSubKey($".{ext}", false))
            {
                if (key == null) return $"{ext.ToUpper()} {GetStaticFileDescription()}".Trim();

                using (var subKey = key.OpenSubKey("OpenWithProgids"))
                {
                    var names = subKey?.GetValueNames();
                    if (names == null || names.Length == 0) return $"{ext.ToUpper()} {GetStaticFileDescription()}".Trim();

                    foreach (var name in names)
                    {
                        retVal = ReadDefaultValue(name);
                        if (!string.IsNullOrEmpty(retVal)) return retVal;
                    }
                }
            }

            return $"{ext.ToUpper()} {GetStaticFileDescription()}".Trim();
        }

        public static string GetStaticFileDescription()
        {
            if (_staticFileDesc == null)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templateFile");
                File.WriteAllText(path, "");
                _staticFileDesc = NativeMethods.GetShellFileType(path);
            }

            return _staticFileDesc;
        }

        public static string GetStaticDirDescription()
        {
            if (_staticFolderDesc == null)
            {
                _staticFolderDesc = "文件夹";
                //_staticFolderDesc = NativeMethods.GetShellFileType(AppDomain.CurrentDomain.BaseDirectory);
            }

            return _staticFolderDesc;
        }

        private static string ReadDefaultValue(string regKey)
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(regKey, false))
            {
                if (key != null)
                {
                    return key.GetValue("") as string;
                }
            }

            return null;
        }
    }
}
