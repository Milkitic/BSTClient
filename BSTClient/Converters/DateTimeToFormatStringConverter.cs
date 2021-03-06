﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BSTClient.Converters
{
    public class DateTimeToFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                try
                {
                    if (dt == DateTime.MinValue) return "未知";
                    string format = parameter is string s ? s : null;
                    return string.IsNullOrEmpty(format)
                        ? dt.ToString(CultureInfo.CurrentCulture)
                        : dt.ToString(format);
                }
                catch (Exception e)
                {
                    return dt.ToString(CultureInfo.CurrentCulture);
                }
            }

            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}