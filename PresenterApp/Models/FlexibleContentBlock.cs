// File: Models/FlexibleContentBlock.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;

namespace PresenterApp.Models
{
    // Định nghĩa các loại khối
    public enum ContentBlockType
    {
        NamedText,
        Image,
        Pdf
    }

    public partial class FlexibleContentBlock : ObservableObject
    {
        [ObservableProperty]
        ContentBlockType type;

        // --- Dùng cho NamedText ---
        [ObservableProperty]
        string name = string.Empty; // "1.", "ĐK", "Năm A"...

        [ObservableProperty]
        string content = string.Empty; // Nội dung...

        // --- Dùng cho Image/Pdf ---
        [ObservableProperty]
        string filePath = string.Empty;

        // --- Thuộc tính trợ giúp (không lưu vào JSON) ---
        [JsonIgnore]
        public string FileName => Path.GetFileName(FilePath);
        [JsonIgnore]
        public bool IsNamedText => Type == ContentBlockType.NamedText;
        [JsonIgnore]
        public bool IsImage => Type == ContentBlockType.Image;
        [JsonIgnore]
        public bool IsPdf => Type == ContentBlockType.Pdf;
    }
}