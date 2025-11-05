// File: Models/ContentEntryTag.cs
using SQLite;

namespace PresenterApp.Models
{
    [Table("ContentEntryTags")]
    public class ContentEntryTag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int ContentEntryId { get; set; }

        [Indexed]
        public int TagId { get; set; }
    }
}