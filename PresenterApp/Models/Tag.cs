// File: Models/Tag.cs
using SQLite;

namespace PresenterApp.Models
{
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}