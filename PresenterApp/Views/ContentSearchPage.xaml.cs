// File: Views/ContentSearchPage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp.Views;

public partial class ContentSearchPage : ContentPage
{
    private readonly ContentSearchSharedViewModel _viewModel;

    public ContentSearchPage(ContentSearchSharedViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Khởi tạo ViewModel ở chế độ tìm kiếm chung (không có sách cụ thể)
        await _viewModel.InitializeAsync(specificBookContext: null);
    }
}