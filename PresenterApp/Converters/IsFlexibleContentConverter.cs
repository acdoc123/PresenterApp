// File: Converters/IsFlexibleContentConverter.cs
using PresenterApp.Models;
using System.Globalization;

namespace PresenterApp.Converters
{
    public class IsFlexibleContentConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is FieldType type && type == FieldType.FlexibleContent;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}