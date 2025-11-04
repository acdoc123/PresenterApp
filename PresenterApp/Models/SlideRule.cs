// File: Models/SlideRule.cs
namespace PresenterApp.Models
{
    public class SlideRule
    {
        public string TargetType { get; set; } = string.Empty;
        public int LinesPerSlide { get; set; }
        public bool IsBold { get; set; }
        public int FontSize { get; set; }
    }
}