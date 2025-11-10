// File: Views/CreatePresentationPage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp.Views;

public partial class CreatePresentationPage : ContentPage
{
    private readonly CreatePresentationViewModel _viewModel;
    public CreatePresentationPage(CreatePresentationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Tải lại Themes và Structures mỗi khi trang xuất hiện
        _viewModel.LoadDataCommand.Execute(null);
    }
}