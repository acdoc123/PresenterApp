// File: Models/SlideDefinition.cs
namespace PresenterApp.Models
{
    // Lớp đại diện trung gian cho một slide, độc lập với định dạng đầu ra
    public class SlideDefinition
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Lines { get; set; } = new List<string>();
        public bool IsBold { get; set; }
        public int FontSize { get; set; }
    }
}