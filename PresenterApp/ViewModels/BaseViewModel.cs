// File: ViewModels/BaseViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiSongGenerator.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]

        bool isBusy;

        [ObservableProperty]
        // Sửa lỗi: Khởi tạo giá trị mặc định
        string title = string.Empty;
        public bool IsNotBusy => !IsBusy;
    }
}