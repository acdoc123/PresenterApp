// File: Models/Book.cs
using SQLite;
using System.Collections.Generic;

namespace PresenterApp.Models
{
    public class Book
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Indexed]
        public int BookTypeId { get; set; }
    }
}