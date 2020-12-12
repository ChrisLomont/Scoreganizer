using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Lomont.Scoreganizer.Core.Model;

namespace Lomont.Scoreganizer.WPF.Views
{
    class SongImageConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is SongData sd)
            {
                if (sd.UIHolder != null)
                    return sd.UIHolder;

                // see if there is an image
                foreach (var f in sd.Files)
                {
                    if (IsImageFilename(f.Filename))
                    {
                        var img = BitmapConverter.BitmapToBitmapImage(new Bitmap(f.Filename));
                        sd.UIHolder = img; // cache it
                        return img;
                    }
                }
            }
            return null;
        }

        bool IsImageFilename(string filename)
        {
            var fl = filename.ToLower();
            if (fl.EndsWith(".png"))
                return true;
            if (fl.EndsWith(".bmp"))
                return true;
            if (fl.EndsWith(".jpg"))
                return true;
            if (fl.EndsWith(".gif"))
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
