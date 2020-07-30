using BSTClient.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BSTClient.API.Models.Response;
using Newtonsoft.Json;

namespace BSTClient.API
{
    public class MultipartUploadClient : IDisposable
    {
        private static readonly string ExecutablePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extensions", "CLIClient", "CLIClient.exe");
        public async Task<string> UploadFileAsync(
            string url,
            IDictionary<string, string> headerData,
            IDictionary<string, string> formData,
            Encoding encoding,
            IList<string> filePaths,
            FileUploadCallback callback)
        {
            if (!File.Exists(ExecutablePath))
                throw new FileNotFoundException("The executable client was not found in: " + ExecutablePath);
            var sb = new StringBuilder();
            sb.Append($"--uri {url} ");

            foreach (var kvp in formData)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                sb.Append($"--header {key}:{value} ");
            }

            foreach (var kvp in formData)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                sb.Append($"--form {key}:{value} ");
            }

            foreach (var filePath in filePaths)
            {
                var fixedFilePath = filePath.Contains(" ")
                    ? $"\"{filePath}\""
                    : filePath;
                sb.Append($"--file {fixedFilePath} ");
            }

            string arguments = sb.ToString();
            var proc = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = ExecutablePath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                }
            };

            string errMsg = null;
            bool isResultReady = false;
            var resultSb = new StringBuilder();
            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.ErrorDataReceived += Proc_ErrorDataReceived;
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            await proc.WaitForExitAsync();

            if (errMsg != null) throw new Exception(errMsg);

            void Proc_OutputDataReceived(object obj, DataReceivedEventArgs args)
            {
                var uploadingFlag = "uploading: ";
                if (args.Data?.StartsWith(uploadingFlag) == true)
                {
                    var len = uploadingFlag.Length;
                    var data = args.Data.Substring(len);
                    var split = data.Split(';')
                        .Select(k => k.Split('=')[1])
                        .ToArray();
                    var current = long.Parse(split[0]);
                    var total = long.Parse(split[1]);
                    var floatPercent = double.Parse(split[2].TrimEnd('%', ' ')) / 100;
                    callback?.Invoke(total, current);
                }
                else if (args.Data == "finished")
                {
                    isResultReady = true;
                }
                else if (isResultReady)
                {
                    resultSb.AppendLine(args.Data);
                }

                Console.WriteLine(args.Data);
            }

            void Proc_ErrorDataReceived(object obj, DataReceivedEventArgs args)
            {
                if (errMsg == null)
                {
                    var errorFlag = "error: ";
                    var responseFlag = "response: ";
                    if (args.Data?.StartsWith(errorFlag) == true)
                    {
                        var data = args.Data.Substring(errorFlag.Length);
                        if (data?.StartsWith(responseFlag) == true)
                        {
                            var data2 = data.Substring(responseFlag.Length);
                            var jsonc = JsonConvert.DeserializeObject<ResponseDataBase<object>>(data2);
                            errMsg = jsonc.Message;
                        }
                        else
                        {
                            errMsg = data;
                        }
                    }
                }
            }

            return resultSb.ToString();
        }

        public void Dispose()
        {
        }
    }

    public static class WebClientExtensions
    {
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public static async Task<string> UploadFileAsync(this HttpClient httpClient, string url,
            FileUploadCallback callback = null,
            params string[] filePaths)
        {
            return await UploadFileAsync(httpClient, url, null, filePaths, callback).ConfigureAwait(false);
        }

        public static async Task<string> UploadFileAsync(this HttpClient httpClient, string url,
            IDictionary<string, string> formData,
            FileUploadCallback callback = null,
            params string[] filePaths)
        {
            return await UploadFileAsync(httpClient, url, formData, filePaths, callback).ConfigureAwait(false);
        }

        public static async Task<string> UploadFileAsync(this HttpClient httpClient, string url,
            IDictionary<string, string> formData,
            IList<string> filePaths,
            FileUploadCallback callback = null)
        {
            return await UploadFileAsync(httpClient, url, formData, DefaultEncoding, filePaths, callback).ConfigureAwait(false);
        }

        public static async Task<string> UploadFileAsync(this HttpClient httpClient,
            string url,
            IDictionary<string, string> formData,
            Encoding encoding,
            IList<string> filePaths,
            FileUploadCallback callback = null)
        {
            var headerData = httpClient?.DefaultRequestHeaders.ToDictionary(
                k => k.Key, k => k.Value.LastOrDefault());

            using (var client = new MultipartUploadClient())
            {
                return await client.UploadFileAsync(url, headerData, formData, encoding, filePaths, callback)
                    .ConfigureAwait(false);
            }
        }
    }

    public delegate void FileUploadCallback(long total, long current);
}
