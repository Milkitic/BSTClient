using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BSTClient.Helpers
{
    public static class StringExtensions
    {
        public static string TrimStart(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (suffixToRemove != null && input.StartsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(suffixToRemove.Length);
            }

            return input;
        }
        public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }

            return input;
        }
    }

    public static class ObjectExtensions
    {
        public static FrameworkElement FindParentObjects(this FrameworkElement obj, params Type[] types)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is FrameworkElement fe)
                {
                    if (types.Length == 0)
                        return fe;

                    var type = fe.GetType();
                    if (types.Any(k => type.IsSubclassOf(k) || k == type))
                    {
                        return fe;
                    }
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}