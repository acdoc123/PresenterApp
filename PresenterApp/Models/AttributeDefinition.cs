// File: Models/AttributeDefinition.cs
using SQLite;

namespace PresenterApp.Models
{
    public class AttributeDefinition
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public FieldType Type { get; set; }

        // Thuộc tính chung (của Loại sách)
        [Indexed]
        public int? BookTypeId { get; set; }

        // Thuộc tính riêng (của Sách)
        [Indexed]
        public int? BookId { get; set; }
    }
}