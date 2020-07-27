using BSTClient.API.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BSTClient.API
{
    public class Requester
    {
        private HttpClient _hc = new HttpClient();

        public Requester()
        {

        }

        public async Task<(bool success, string message)> AuthenticationAsync(string user, string pass)
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
                    return (false, message);
            }

            var content = await result.Content.ReadAsStringAsync();
            if (code != HttpStatusCode.OK)
            {
                return (true, content);
            }

            return (false, content);
        }
    }
}
