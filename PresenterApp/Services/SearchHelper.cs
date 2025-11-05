// File: Services/SearchHelper.cs
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PresenterApp.Services
{
    public static class SearchHelper
    {
        // Biểu thức chính quy để loại bỏ ký tự không mong muốn
        private static readonly Regex _invalidCharsRegex = new Regex(@"[-,.!?'""]", RegexOptions.Compiled);
        // Biểu thức chính quy để chuẩn hóa khoảng trắng
        private static readonly Regex _whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public static string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 1. Chuyển sang chữ thường
            string normalized = input.ToLowerInvariant();

            // 2. Loại bỏ dấu (ví dụ: "Nội dung" -> "Noi dung")
            normalized = RemoveAccents(normalized);

            // 3. Loại bỏ các ký tự đặc biệt (dấu phẩy, chấm than, gạch ngang...)
            normalized = _invalidCharsRegex.Replace(normalized, " ");

            // 4. Chuẩn hóa khoảng trắng (thay thế nhiều khoảng trắng bằng 1)
            normalized = _whitespaceRegex.Replace(normalized, " ").Trim();

            return normalized;
        }

        private static string RemoveAccents(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}