using BSTClient.API.Models;
using BSTClient.API.Models.Response;
using BSTClient.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BSTClient.API
{
    public class Requester
    {
        private HttpClient _hc = new HttpClient();
        private bool _initialized;

        public string Host { get; set; } = "http://localhost:27001/";

        public static Requester Default { get; private set; }

        public Requester()
        {
            if (UserPasswordManager.TryGet(out string uname, out string pword))
            {
                Initialize(uname, pword, false);
            }

            Default = this;
        }

        public async Task<string> TestAuthenticationAsync()
        {
            try
            {
                var result = await _hc.GetAsync(
                    $"{Host}api/authenticate/test"
                );

                if (GetUnhandledMessage(result, out var message)) return message;
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> AuthenticationAsync(string user, string pass)
        {
            try
            {
                var reqJson = JsonConvert.SerializeObject(new AuthenticateModel(user, pass));

                var result = await _hc.PostAsync(
                    $"{Host}api/authenticate",
                    GetJsonContent(reqJson)
                );

                if (GetUnhandledMessage(result, out var message)) return message;

                var respJson = await result.Content.ReadAsStringAsync();
                if (result.StatusCode == HttpStatusCode.OK &&
                    ResponseBase.GetCode(respJson) == "200")
                {
                    Initialize(user, pass, !_initialized);
                    return null;
                }

                return GetJsonMessage(respJson);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<(bool success, string message, NavObj)> GetNavigatorInfo()
        {
            try
            {
                var result = await _hc.GetAsync(
                    $"{Host}api/navigator"
                );

                if (GetUnhandledMessage(result, out var message)) return (false, message, null);
                var respJson = await result.Content.ReadAsStringAsync();
                if (result.StatusCode == HttpStatusCode.OK &&
                    ResponseBase.GetCode(respJson) == "200")
                {
                    var obj = JsonConvert.DeserializeObject<ResponseDataBase<NavObj>>(respJson);
                    return (true, obj.Message, obj.Data);
                }

                return (false, GetJsonMessage(respJson), null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        private static string GetJsonMessage(string json)
        {
            var jsonCode = ResponseBase.GetCode(json);
            var jsonMessage = ResponseBase.GetMessage(json);
            if (jsonCode != "400.1") return jsonMessage;

            var kvp = JsonConvert.DeserializeObject<ResponseDataBase<Dictionary<string, string>>>(json);
            return jsonMessage + ":" + Environment.NewLine + string.Join(Environment.NewLine,
                kvp.Data.Select(k => k.Key + ": " + k.Value));
        }

        private static bool GetUnhandledMessage(HttpResponseMessage result, out string s)
        {
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                if (result.StatusCode != HttpStatusCode.BadRequest)
                {
                    s = ex.Message;
                    return true;
                }
            }

            s = null;
            return false;
        }

        private static StringContent GetJsonContent(string json)
        {
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private void Initialize(string user, string pass, bool update)
        {
            var bytes = Encoding.UTF8.GetBytes(user + ":" + pass);
            var base64 = Convert.ToBase64String(bytes);
            var g = new AuthenticationHeaderValue("Basic", base64);
            _hc.DefaultRequestHeaders.Authorization = g;
            if (update)
                UserPasswordManager.Set(user, pass);

            _initialized = true;
        }
    }
}
