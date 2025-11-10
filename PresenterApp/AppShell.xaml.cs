// File: AppShell.xaml.cs
namespace PresenterApp;
using PresenterApp.Views;
using Syncfusion.Maui.Toolkit.SegmentedControl;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        InitializeComponent();
        var currentTheme = Application.Current!.RequestedTheme;
        ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;

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

    private void SfSegmentedControl_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        Application.Current!.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
    }
}