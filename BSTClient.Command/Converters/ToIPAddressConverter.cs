using System.Net;

namespace BSTClient.Command.Converters
{
    internal class ToIPAddressConverter : ValueConverter<IPAddress>
    {
        public override IPAddress Convert(string s)
        {
            return IPAddress.Parse(s);
        }
    }
}