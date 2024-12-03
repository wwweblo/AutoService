using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AutoService.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imagePath && !string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute); 
                    image.EndInit();
                    return image; 
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
