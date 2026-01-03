// File: Models/PresentationSlideItem.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace PresenterApp.Models
{
    // Đại diện cho một mục trong "Danh sách phát" của phiên làm việc hiện tại
    public partial class PresentationSlideItem : ObservableObject
    {
        // Có thể null nếu là mục tự thêm (adhoc)
        public PresentationComponent? SourceComponent { get; set; }

        [ObservableProperty]
        string content = string.Empty;

        // Nội dung gốc (nếu được link với ContentEntry) - dùng để restore hoặc check
        public ContentEntry? SourceContentEntry { get; set; }

        // --- Style Overrides (User chỉnh sửa trên UI) ---
        [ObservableProperty]
        float fontSize = 32;

        [ObservableProperty]
        string fontColor = "#000000";

        [ObservableProperty]
        string textAlignment = "Center"; // Center, Start, End

        [ObservableProperty]
        string fontFamily = "Arial";

        [ObservableProperty]
        float lineSpacing = 1.0f;

        // --- Splitting Settings ---
        [ObservableProperty]
        bool autoSplit = true;

        [ObservableProperty]
        string splitCharacter = "---";

        public PresentationSlideItem()
        {
        }
    }
}