using BSTClient;
using ConsoleApp1.MultipartUpload;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var result1 = McGenerator.GetMachineCode();
            //HttpHelper.HttpUploadFile("http://localhost:27001/api/explorer/upload",
            //    @"E:\MIS_Tianziling_Backup\20200526132414.zip", new NameValueCollection()
            //    {
            //        ["remark"] = "test"
            //    });
            var files = new DirectoryInfo(@"E:\test\source").GetFiles();
            OptimizedHttpHelper.UploadFileAsync("http://localhost:27001/api/explorer/upload",
                null,
                null,
                Encoding.Default,
                files.Select(k => k.FullName).ToArray(), Upload).Wait();
            Console.WriteLine("Hello World!");
        }

        private static string _currentStr;
        private static void Upload(int index, long total, long current)
        {
            var currentStr = $"{current / (double)total:P}";
            if (currentStr == _currentStr) return;
            Console.WriteLine($"{index}: {currentStr}");
            _currentStr = currentStr;
        }
    }
}
