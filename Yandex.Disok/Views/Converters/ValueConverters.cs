using System;
using System.Collections;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Ya.D.Helpers;
using Ya.D.Models;

namespace Ya.D.Views.Converters
{
    public class ItemExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is DiskItem item) || !(value is string path))
                return "?";
            if (item != null && item.IsFolder)
                return string.Empty;
            if (item != null)
                return !item.Path.Contains(".") ? "?" : item.Path.Split('.').LastOrDefault();
            return !path.Contains(".") ? "?" : path.Split('.').LastOrDefault();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class MemorySizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return $"0 kB";
            double size = 0;
            double.TryParse(value.ToString(), out size);
            if (size > 1024 * 1024 * 1024)
                return $"{size / (1024 * 1024 * 1024):N2} GB";
            else if (size > 1024 * 1024)
                return $"{size / (1024 * 1024):N2} MB";
            else if (size > 1024)
                return $"{size / 1024:N2} kB";
            return $"{size} B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class BytesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[] image)
                return DataUtils.SaveToImageSource(image);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var result = false;
            if (value == null)
                return result;

            switch (value)
            {
                case int typeValue:
                    result = typeValue > 0;
                    break;
                case long typeValue:
                    result = typeValue > 0;
                    break;
                case decimal typeValue:
                    result = typeValue > 0;
                    break;
                case double typeValue:
                    result = typeValue > 0;
                    break;
                case string typeValue:
                    result = !string.IsNullOrEmpty(typeValue);
                    break;
                case ICollection typeValue:
                    result = typeValue.Count > 0;
                    break;
                case Visibility typeValue:
                    result = typeValue == Visibility.Visible;
                    break;
                default:
                    result = true;
                    break;
            }
            if (parameter != null)
            {
                if (parameter is bool typedParam)
                {
                    result &= typedParam;
                }
                else
                {
                    result = value.GetType().Name == parameter.ToString() ? true : false;
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class ObjectToBoolReverseConverter : IValueConverter
    {
        private ObjectToBoolConverter converter = new ObjectToBoolConverter();

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return !(bool)converter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return !(bool)converter.Convert(value, targetType, parameter, culture);
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;
            if (parameter == null)
                return value.ToString();
            return string.Format(parameter.ToString(), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class PageSettingsToModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is BrowseSettings settings) || settings == null)
                return ListViewSelectionMode.None;
            if (settings.SelectFolder)
                return ListViewSelectionMode.Single;
            if (settings.SelectMultipleFiles)
                return ListViewSelectionMode.Multiple;
            if (settings.SelectFiles)
                return ListViewSelectionMode.Single;
            return ListViewSelectionMode.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new BrowseSettings();
        }
    }

    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;

            if (value is bool) return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            if (value.ToString() == "0")
                return Visibility.Collapsed;

            if (value is int)
            {
                int tmp = 0;
                return int.TryParse(value.ToString(), out tmp) ? (tmp > 0 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
            }
            if (value is long)
            {
                long tmp = 0;
                return long.TryParse(value.ToString(), out tmp) ? (tmp > 0 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
            }
            if (value is decimal)
            {
                decimal tmp = 0;
                return decimal.TryParse(value.ToString(), out tmp) ? (tmp > 0 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
            }
            if (value is double)
            {
                double tmp = 0;
                return double.TryParse(value.ToString(), out tmp) ? (tmp > 0 ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
            }
            if (value is string) return !string.IsNullOrEmpty(value.ToString()) ? Visibility.Visible : Visibility.Collapsed;

            if (string.IsNullOrEmpty(value.ToString()))
                return Visibility.Collapsed;
            if (value is Visibility)
                return (Visibility)value;
            if (value is ICollection)
                return (value as ICollection).Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (parameter != null)
                return value.GetType().Name == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;

            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueToVisibilityReverseConverter : IValueConverter
    {
        private readonly ValueToVisibilityConverter _converter = new ValueToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)_converter.Convert(value, targetType, parameter, language) == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return _converter.ConvertBack(value, targetType, parameter, language);
        }
    }
}
