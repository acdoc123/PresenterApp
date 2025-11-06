// File: Models/PresentationComponent.cs
using SQLite;

namespace PresenterApp.Models
{
    // Đại diện cho một "Thành phần" trong "Cấu trúc Trình chiếu"
    public class PresentationComponent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int StructureId { get; set; } // Liên kết với PresentationStructure
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } // Để sắp xếp
    }
}