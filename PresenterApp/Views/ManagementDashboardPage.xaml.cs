// File: Views/ManagementDashboardPage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp.Views;

public partial class ManagementDashboardPage : ContentPage
{
    private readonly ManagementDashboardViewModel _viewModel;
    public ManagementDashboardPage(ManagementDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Tải lại dữ liệu mỗi khi quay lại trang này
        _viewModel.LoadDataCommand.Execute(null);
    }
}