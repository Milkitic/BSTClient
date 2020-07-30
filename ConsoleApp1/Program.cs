using BSTClient;
using ConsoleApp1.MultipartUpload.WebClientImpl;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp1.MultipartUpload;
using ConsoleApp1.MultipartUpload.OriginImpl;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var result1 = McGenerator.GetMachineCode();
            var files = new DirectoryInfo(@"E:\test\source").GetFiles();
            var filePaths = files.Select(k => k.FullName).ToArray();
            var uri = "http://localhost:27001/api/explorer/upload";
            //var uri = "http://milkitic.name:27001/api/explorer/upload";
            var path = @"E:\Virtual Machines\KUbuntu x64\KUbuntu x64-s008.vmdk";
            //OptimizedHttpHelper.UploadFileAsync(uri,
            //    null,
            //    null,
            //    Encoding.Default,
            //    filePaths/*.Take(1).ToList()*/, Upload).Wait();
            OptimizedHttpHelper.UploadFileAsync(uri,
                null,
                null,
                Encoding.Default,
                new[] { path }/*.Take(1).ToList()*/, Upload).Wait();
            //Task.Run(async () =>
            //{
            //    using (var webClient = new WebClient())
            //    {
            //        webClient.UploadProgressChanged += WebClient_UploadProgressChanged;
            //        webClient.UploadDataCompleted += WebClient_UploadDataCompleted;
            //        var multipartFormBuilder = new MultipartFormBuilder();
            //        var path = @"E:\Virtual Machines\KUbuntu x64\KUbuntu x64-s008.vmdk";
            //        //var path =
            //        //    @"D:\ProgramData\Microsoft\VisualStudio\Packages\Microsoft.Net.Core.SDK.2.1,version=16.2.29116.96,chip=x64\dotnet-sdk-2.1.801-win-x64.exe";
            //        //var path = filePaths.First();
            //        multipartFormBuilder.AddFile(new FileInfo(path));
            //        //await webClient.UploadFileTaskAsync(uri, path);
            //        await webClient.UploadMultipartAsync(uri, multipartFormBuilder);
            //    }
            //}).Wait();
            Console.WriteLine("Hello World!");
        }

        private static void WebClient_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            //Console.WriteLine(JsonConvert.SerializeObject(e));
        }

        private static void WebClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            Console.WriteLine("{0}    uploaded {1} of {2} bytes. {3} % complete...",
                e.UserState?.ToString(),
                e.BytesSent,
                e.TotalBytesToSend,
                e.ProgressPercentage);
            //Console.WriteLine(JsonConvert.SerializeObject(new
            //{ e.BytesReceived, e.BytesSent, e.TotalBytesToReceive, e.TotalBytesToSend, e.ProgressPercentage }));
            //Console.WriteLine($"{e.ProgressPercentage}%");
        }

        private static string _currentStr;
        private static void Upload(int index, long total, long current)
        {
            var currentStr = $"{current / (double)total:P0}";
            if (currentStr == _currentStr) return;
            Console.WriteLine($"{index}: {currentStr}");
            _currentStr = currentStr;
        }
    }
}
