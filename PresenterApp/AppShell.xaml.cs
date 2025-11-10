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
        Routing.RegisterRoute(nameof(EditTagPage), typeof(EditTagPage));
        Routing.RegisterRoute(nameof(EditContentEntryPage), typeof(EditContentEntryPage));
        Routing.RegisterRoute(nameof(TagSelectionPage), typeof(TagSelectionPage));
        Routing.RegisterRoute(nameof(EditThemePage), typeof(EditThemePage));
        Routing.RegisterRoute(nameof(EditStructurePage), typeof(EditStructurePage));

        Routing.RegisterRoute(nameof(CreatePresentationPage), typeof(CreatePresentationPage));
        Routing.RegisterRoute(nameof(ContentSearchPage), typeof(ContentSearchPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    }
}