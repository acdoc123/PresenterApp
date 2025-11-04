// File: AppShell.xaml.cs
using PresenterApp.Views;

namespace PresenterApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Đăng ký route cho trang chi tiết
        Routing.RegisterRoute(nameof(SongDetailPage), typeof(SongDetailPage));
    }
}