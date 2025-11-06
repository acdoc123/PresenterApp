// File: Models/PresentationStructure.cs
using SQLite;

namespace PresenterApp.Models
{
    public class PresentationStructure
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}