// File: Models/Song.cs
using SQLite;

namespace PresenterApp.Models
{
    public class Song
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Lyrics { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}