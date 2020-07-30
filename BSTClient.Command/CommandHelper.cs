using BSTClient.Command.Converters;
using System;
using System.Net;

namespace BSTClient.Command
{
    public class CommandHelper
    {
        public static T ReadValue<T>(string[] array, ref int i)
        {
            if (array.Length <= i + 1)
            {
                throw new ArgumentException("value required");
            }

            var s = array[i + 1];
            if (s.StartsWith("--") || s.StartsWith("-"))
            {
                throw new ArgumentException("value required");
            }

            i++;

            var parser = GetRegisteredParser<T>();

            return (T)parser.Convert(s, typeof(T));
        }

        private static IValueConverter GetRegisteredParser<T>()
        {
            var t = typeof(T);
            IValueConverter value = null;
            if (t == typeof(string))
            {
                value = new ToStringConverter();
            }
            else if (t == typeof(IPAddress))
            {
                value = new ToIPAddressConverter();
            }
            else
            {
                if (ToNumberConverter.IsNumberType(t))
                {
                    value = new ToNumberConverter();
                }
            }

            if (value == null)
                throw new ArgumentOutOfRangeException(null, $"There is no parser to {t.FullName}.");

            return value;
        }
    }
}
