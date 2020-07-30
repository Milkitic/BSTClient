using System;

namespace BSTClient.Command.Converters
{
    internal interface IValueConverter
    {
        object Convert(string s, Type type);
    }

    internal interface IValueConverter<out T>
    {
        T Convert(string s);
    }
}