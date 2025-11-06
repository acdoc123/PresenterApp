// File: Models/PresentationTheme.cs
using SQLite;

namespace PresenterApp.Models
{
    public class PresentationTheme
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Kích thước (lưu bằng inch)
        public double SlideWidth { get; set; } = 10; // 16:9 (Ngang)
        public double SlideHeight { get; set; } = 5.625; // 16:9 (Ngang)
        public SlideOrientation Orientation { get; set; } = SlideOrientation.Landscape;

        // Màu sắc (lưu mã hex, ví dụ: #FFFFFF)
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string TextColor { get; set; } = "#000000";

        // Phông chữ (lưu tên font)
        public string TitleFont { get; set; } = "Arial";
        public string ContentFont { get; set; } = "Calibri";
    }
}