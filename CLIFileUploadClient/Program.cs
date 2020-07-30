using BSTClient.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CLIFileUploadClient
{
    class Program
    {
        private const string ErrorPrefix = "error: ";
        private static string _lastPercent;
        private static uint _accuracy = 1;

        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                var paths = new List<string>();
                var headers = new Dictionary<string, string>();
                var formData = new Dictionary<string, string>();
                string uri = null;

                try
                {
                    for (var i = 0; i < args.Length; i++)
                    {
                        var arg = args[i];
                        switch (arg)
                        {
                            case "--uri":
                                uri = CommandHelper.ReadValue<string>(args, ref i);
                                break;
                            case "--file":
                                var file = CommandHelper.ReadValue<string>(args, ref i);
                                if (!File.Exists(file)) return ExitWithError($"File not exist: {file}");
                                paths.Add(file);
                                break;
                            case "--header":
                                var header = CommandHelper.ReadValue<string>(args, ref i);
                                var kvp = GetKeyValue(header);
                                headers.Add(kvp.Key, kvp.Value);
                                break;
                            case "--form":
                                var form = CommandHelper.ReadValue<string>(args, ref i);
                                var formKvp = GetKeyValue(form);
                                formData.Add(formKvp.Key, formKvp.Value);
                                break;
                            case "-p":
                                _accuracy = CommandHelper.ReadValue<uint>(args, ref i);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                    return UsageErrorMessage();
                }

                if (_accuracy > 14)
                {
                    return ExitWithError("percent_accuracy should be smaller than 15");
                }

                if (uri == null || paths.Count == 0)
                {
                    if (uri == null)
                    {
                        Console.Error.WriteLine(ErrorPrefix + "please specify server uri.");
                    }
                    else
                    {
                        Console.Error.WriteLine(ErrorPrefix + "please specify at least one file.");
                    }

                    return UsageErrorMessage();
                }

                var result = MultipartUploadClient.UploadFileAsync(uri,
                    headers,
                    formData,
                    Encoding.Default,
                    paths, OnDataTransferred).Result;
                Console.WriteLine("finished");
                Console.WriteLine(result);
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(ErrorPrefix + e.InnerException?.Message ?? e.Message);
                return 1;
            }
        }

        private static int UsageErrorMessage()
        {
            Console.Error.WriteLine("usage: ." + Path.DirectorySeparatorChar +
                                    Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) +
                                    " --uri [server_uri]" +
                                    " --file [file_path]..." +
                                    " --header [key]:[value]..." +
                                    " --form [key]:[value]..." +
                                    " -p [percent_accuracy]");
            return 1;
        }

        private static void OnDataTransferred(int index, long total, long current)
        {
            var currentStr = (current / (double)total).ToString("P" + _accuracy);
            if (currentStr == _lastPercent) return;
            Console.WriteLine($"uploading: transferred={current}; total={total}; percent={currentStr}");
            _lastPercent = currentStr;
        }

        private static KeyValuePair<string, string> GetKeyValue(string header)
        {
            var firstColon = header.IndexOf(':');
            if (firstColon == -1)
                return new KeyValuePair<string, string>(header, "");

            var key = header.Substring(0, firstColon);
            var value = header.Substring(firstColon + 1);
            return new KeyValuePair<string, string>(key, value);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            var ret = ExitWithError(exception.InnerException?.Message ?? exception.Message);
            Environment.Exit(ret);
        }

        private static int ExitWithError(string message)
        {
            Console.Error.WriteLine(ErrorPrefix + message);
            return 1;
        }
    }
}
