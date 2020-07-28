using Newtonsoft.Json.Linq;

namespace BSTClient.API.Models.Response
{
    public class ResponseDataBase<T> : ResponseBase
    {
        public T Data { get; set; }
    }
    public class ResponseBase
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public static string GetCode(string rawJson)
        {
            var json = JObject.Parse(rawJson);
            return json["code"].Value<string>();
        }

        public static string GetMessage(string rawJson)
        {
            var json = JObject.Parse(rawJson);
            return json["message"].Value<string>();
        }
    }
}
