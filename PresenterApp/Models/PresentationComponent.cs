// File: Models/PresentationComponent.cs
using SQLite;
using System.ComponentModel.DataAnnotations.Schema;

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
                                              // Thuộc tính để lưu trữ bộ lọc tùy chọn
        public int? BookTypeId { get; set; }
        [ForeignKey(nameof(BookTypeId))]
        public BookType? BookType { get; set; }

        public int? BookId { get; set; }
        [ForeignKey(nameof(BookId))]
        public Book? Book { get; set; }

        public int? TagId { get; set; }
        [ForeignKey(nameof(TagId))]
        public Tag? Tag { get; set; }
    }
}