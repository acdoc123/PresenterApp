// File: Models/NamedTextEntry.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace PresenterApp.Models
{
    /// <summary>
    /// Đại diện cho một mục con có tên và nội dung
    /// (ví dụ: [1.] [Nội dung lời 1])
    /// </summary>
    public partial class NamedTextEntry : ObservableObject
    {
        [ObservableProperty]
        string name; // Tên (ví dụ: "1", "ĐK", "Năm A")

        [ObservableProperty]
        string content; // Nội dung (ví dụ: "Nội dung của lời 1...")
    }
}