using BSTClient;
using System;
using System.Collections.Specialized;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var result1 = McGenerator.GetMachineCode();
            HttpHelper.HttpUploadFile("http://localhost:27001/api/explorer/upload",
                @"E:\MIS_Tianziling_Backup\20200526132414.zip", new NameValueCollection()
                {
                    ["remark"] = "test"
                });
            Console.WriteLine("Hello World!");
        }
    }
}
