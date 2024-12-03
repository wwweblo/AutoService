using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorName)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(colorName);
            }
            catch
            {
                return Colors.Transparent; // Если цвет недействителен
            }
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
