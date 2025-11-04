// File: Models/PresentationTemplate.cs
namespace PresenterApp.Models
{
    public class PresentationTemplate
    {
        public string Name { get; set; } = string.Empty;
        public List<SlideRule> Rules { get; set; } = new List<SlideRule>();
        // Sửa lỗi: Khởi tạo DefaultRule để tránh cảnh báo null
        public SlideRule DefaultRule { get; set; } = new SlideRule();
    }
}