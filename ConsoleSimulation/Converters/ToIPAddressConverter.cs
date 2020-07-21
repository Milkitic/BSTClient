using System.Net;

namespace ConsoleSimulation.Converters
{
    internal class ToIPAddressConverter : ValueConverter<IPAddress>
    {
        public override IPAddress Convert(string s)
        {
            return IPAddress.Parse(s);
        }
    }
}