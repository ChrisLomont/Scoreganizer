using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;


namespace Lomont.Scoreganizer.WPF.Views
{
    public class FilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filepath)
                return Path.GetFileName(filepath);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
