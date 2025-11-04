// File: Models/ContentEntry.cs
using SQLite;
using System;

namespace PresenterApp.Models
{
    public class ContentEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int BookId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}