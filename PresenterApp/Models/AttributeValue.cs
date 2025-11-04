// File: Models/AttributeValue.cs
using SQLite;

namespace PresenterApp.Models
{
    public class AttributeValue
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ContentEntryId { get; set; }

        [Indexed]
        public int AttributeDefinitionId { get; set; }

        // Lưu trữ tất cả các giá trị dưới dạng string
        // (Text, Number, Path-to-Image, Path-to-Pdf)
        public string Value { get; set; } = string.Empty;
    }
}