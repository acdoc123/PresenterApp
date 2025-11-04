// File: Models/BookTag.cs
using SQLite;

namespace PresenterApp.Models
{
    [Table("BookTags")]
    public class BookTag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Simple PK is easier for SQLite-net

        [Indexed]
        public int BookId { get; set; }
        [Indexed]
        public int TagId { get; set; }
    }
}