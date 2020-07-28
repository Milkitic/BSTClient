using System;
using System.Security.Cryptography;
using System.Text;
using BSTClient;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var result1 = McGenerator.GetMachineCode();
            Console.WriteLine("Hello World!");
        }
    }
}
