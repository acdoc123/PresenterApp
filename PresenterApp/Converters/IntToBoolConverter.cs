// File: Converters/IntToBoolConverter.cs
using System.Globalization;

namespace PresenterApp.Converters
{
    // Converter để ẩn nút "Xóa" khi thêm bài hát mới
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is int intValue && intValue > 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}