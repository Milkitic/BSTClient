using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CLIFileUploadClient
{
    public static class MultipartUploadClient
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

            return await UploadFileAsync(url, headerData, formData, encoding, filePaths, callback).ConfigureAwait(false);
        }

        public static async Task<string> UploadFileAsync(
            string url,
            IDictionary<string, string> headerData,
            IDictionary<string, string> formData,
            Encoding encoding,
            IList<string> filePaths,
            FileUploadCallback callback)
        {
            string boundary = $"---------------------------{DateTime.Now.Ticks:x}";
            byte[] boundaryBytes = Encoding.ASCII.GetBytes($"\r\n--{boundary}\r\n");
            byte[] endBytes = Encoding.ASCII.GetBytes($"\r\n--{boundary}--\r\n");

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.SendChunked = true;
            request.AllowWriteStreamBuffering = false;
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;

            var keys = request.Headers.AllKeys;
            if (headerData != null)
            {
                foreach (var kvp in headerData)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    if (keys.Contains(key))
                    {
                        request.Headers[key] = value;
                    }
                    else
                    {
                        request.Headers.Add(key, value);
                    }
                }
            }

            using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                //1.1 key/value
                if (formData != null)
                {
                    foreach (var key in formData.Keys)
                    {
                        await WriteBoundaryAsync(stream, boundaryBytes).ConfigureAwait(false);
                        await WriteRegularAsync(encoding, key, formData[key], stream).ConfigureAwait(false);
                    }
                }

                //1.2 file
                for (int i = 0; i < filePaths.Count; i++)
                {
                    var filePath = filePaths[i];
                    await WriteBoundaryAsync(stream, boundaryBytes).ConfigureAwait(false);
                    await WriteFileAsync(encoding, $"file{i}", filePath, stream, i, callback).ConfigureAwait(false);
                }

                //1.3 form end
                await stream.WriteAsync(endBytes, 0, endBytes.Length).ConfigureAwait(false);
            }

            //2.WebResponse
            using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) return null;

                using (var sr = new StreamReader(responseStream))
                    return await sr.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private static async Task WriteFileAsync(Encoding encoding, string key, string filePath, Stream stream,
            int i, FileUploadCallback callback)
        {
            const string headerTemplate =
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
            string header = string.Format(headerTemplate, key, Path.GetFileName(filePath));
            byte[] headerBytes = encoding.GetBytes(header);

            await stream.WriteAsync(headerBytes, 0, headerBytes.Length).ConfigureAwait(false);
            byte[] buffer = new byte[4096];
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var total = fileStream.Length;
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                    callback?.Invoke(i, total, fileStream.Position);
                    buffer = new byte[4096];
                }
            }
        }

        private static async Task WriteRegularAsync(Encoding encoding, string key, string value, Stream stream)
        {
            const string formDataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            string formItemStr = string.Format(formDataTemplate, key, value);
            byte[] formItemBytes = encoding.GetBytes(formItemStr);
            await stream.WriteAsync(formItemBytes, 0, formItemBytes.Length).ConfigureAwait(false);
        }

        private static async Task WriteBoundaryAsync(Stream stream, byte[] boundaryBytes)
        {
            await stream.WriteAsync(boundaryBytes, 0, boundaryBytes.Length).ConfigureAwait(false);
        }
    }

    public delegate void FileUploadCallback(int index, long total, long current);
}