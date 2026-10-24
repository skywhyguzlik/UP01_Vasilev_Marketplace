using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MarketplaceApp.Classes
{
    
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            string fullPath = GetFullPath(path);

            if (fullPath == null || !File.Exists(fullPath))
                fullPath = GetFullPath("images/no_photo.png");
            if (!File.Exists(fullPath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;  
                bitmap.UriSource = new Uri(fullPath);
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        
        public static string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            path = path.Replace('/', '\\');
            return Path.IsPathRooted(path) ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}
