// File: MainPage.xaml.cs
using PresenterApp.Views; // Cần thêm namespace này

namespace PresenterApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Thay thế OnCounterClicked
        private async void OnManageClicked(object? sender, EventArgs e)
        {
            // Điều hướng đến trang Bảng điều khiển mới
            await Shell.Current.GoToAsync(nameof(ManagementDashboardPage));
        }
    }
}