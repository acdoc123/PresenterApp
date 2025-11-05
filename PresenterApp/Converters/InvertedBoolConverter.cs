// File: Converters/InvertedBoolConverter.cs
using System.Globalization;

namespace PresenterApp.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Đảo ngược giá trị boolean
            return !(value is bool b && b);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Đảo ngược lại
            return !(value is bool b && b);
        }
    }
}