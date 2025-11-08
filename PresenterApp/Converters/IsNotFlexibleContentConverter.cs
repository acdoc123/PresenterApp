// File: Converters/IsNotFlexibleContentConverter.cs
using PresenterApp.Models;
using System.Globalization;

namespace PresenterApp.Converters
{
    public class IsNotFlexibleContentConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Trả về true cho Text, TextArea, Number
            return value is FieldType type && type != FieldType.FlexibleContent;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}