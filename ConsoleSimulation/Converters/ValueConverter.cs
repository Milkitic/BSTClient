using System;

namespace ConsoleSimulation.Converters
{
    public abstract class ValueConverter : IValueConverter
    {
        public abstract object Convert(string s, Type type);
    }

    public abstract class ValueConverter<T> : IValueConverter, IValueConverter<T>
    {
        public abstract T Convert(string s);

        object IValueConverter.Convert(string s, Type type)
        {
            return Convert(s);
        }
    }
}