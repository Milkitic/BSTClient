namespace BSTClient.Shared
{
    public static class SharedUtils
    {
        public static string CountSize(long size)
        {
            string strSize = "";
            long factSize = size;
            var format = "0.##";
            if (factSize < 1024)
                strSize = factSize.ToString(format) + "B";
            else if (factSize >= 1024 && factSize < 1048576)
                strSize = (factSize / 1024f).ToString(format) + "KB";
            else if (factSize >= 1048576 && factSize < 1073741824)
                strSize = (factSize / 1024f / 1024f).ToString(format) + "MB";
            else if (factSize >= 1073741824)
                strSize = (factSize / 1024f / 1024f / 1024f).ToString(format) + "GB";
            return strSize;
        }
    }
}