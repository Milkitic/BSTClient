using System;
using System.Linq;

namespace BSTClient.Command.Converters
{
    internal class ToNumberConverter : ValueConverter
    {
        public override object Convert(string s, Type type)
        {
            if (!IsNumberType(type)) throw new ArgumentException("Not number type.", nameof(type));
            if (ConvertValue(s, type, out var converted))
                return converted;
            throw new Exception(converted?.ToString());
        }

        public static bool IsNumberType(Type value)
        {
            return value == typeof(sbyte)
                   || value == typeof(byte)
                   || value == typeof(short)
                   || value == typeof(ushort)
                   || value == typeof(int)
                   || value == typeof(uint)
                   || value == typeof(long)
                   || value == typeof(ulong)
                   || value == typeof(float)
                   || value == typeof(double)
                   || value == typeof(decimal);
        }

        private static bool ConvertValue(string value, Type propType, out object converted)
        {
            try
            {
                object arg;
                if (propType == typeof(bool) && int.TryParse(value, out var parsed))
                    arg = parsed;
                else
                    arg = value;

                var type = typeof(Convert);
                var methodName = $"To{propType.Name}";
                var method = type.GetMethods()
                    .FirstOrDefault(t =>
                    {
                        var parameters = t.GetParameters();
                        return t.Name == methodName &&
                               parameters.Length == 1 &&
                               parameters[0].ParameterType == typeof(object);
                    });

                if (method == default)
                {
                    converted = null;
                    return false;
                }

                object[] p = { arg };
                converted = method.Invoke(null, p);
                return true;
            }
            catch (Exception ex)
            {
                converted = ex.InnerException?.Message ?? ex.Message;
                return false;
            }
        }
    }
}