using BSTClient.API.Models.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using BSTClient.Helpers;

namespace BSTClient.Converters
{
    class ItemToFileTypeConverter : IValueConverter
    {
        private static Dictionary<string, string> _fileTypeMapping = new Dictionary<string, string>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryDesc dd)
            {
                return FileNameExtension.GetStaticDirDescription();
            }
            else if (value is FileDesc fd)
            {
                var fixName = fd.Name.TrimEnd("._disabled");
                var ext = Path.GetExtension(fixName);
                if (string.IsNullOrEmpty(ext)) return FileNameExtension.GetStaticFileDescription();

                if (_fileTypeMapping.ContainsKey(ext)) return _fileTypeMapping[ext];
                var desc = FileNameExtension.GetDescription(ext);
                _fileTypeMapping.Add(ext, desc);
                return desc;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
