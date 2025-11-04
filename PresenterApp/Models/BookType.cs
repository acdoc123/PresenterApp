// File: Models/BookType.cs
using SQLite;

namespace PresenterApp.Models
{
    public class BookType
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}