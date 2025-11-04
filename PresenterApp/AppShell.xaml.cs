// File: AppShell.xaml.cs
namespace PresenterApp;
using PresenterApp.Views;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ManagementDashboardPage), typeof(ManagementDashboardPage));
        Routing.RegisterRoute(nameof(EditBookTypePage), typeof(EditBookTypePage));
        Routing.RegisterRoute(nameof(EditBookPage), typeof(EditBookPage));
    }
}