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

        public Requester()
        {
            if (UserPasswordManager.TryGet(out string uname, out string pword))
            {
                Initialize(uname, pword, false);
            }

        }

        public async Task<string> AuthenticationAsync(string user, string pass)
        {
            var json = JsonConvert.SerializeObject(new AuthenticateModel()
            {
                Username = user,
                Password = pass
            });
            var result = await _hc.PostAsync("http://localhost:27001/api/users/authenticate",
                new StringContent(json, Encoding.UTF8, "application/json"));

            string message;
            var code = result.StatusCode;

            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                if (code != HttpStatusCode.BadRequest)
                    return message;
            }

            var content = await result.Content.ReadAsStringAsync();
            if (code == HttpStatusCode.OK)
            {
                Initialize(user, pass, !_initialized);
                return null;
            }

            var jsonCode = ResponseBase.GetCode(content);
            var jsonMessage = ResponseBase.GetMessage(content);
            if (jsonCode == "400.1")
            {
                var d = JsonConvert.DeserializeObject<ResponseDataBase<Dictionary<string, string>>>(content);
                return jsonMessage + ":" + Environment.NewLine + string.Join(Environment.NewLine,
                    d.Data.Select(k => k.Key + ": " + k.Value));
            }

            return jsonMessage;
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
