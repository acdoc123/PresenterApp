// File: Views/HomePage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Sử dụng MainThread để đảm bảo lệnh được thực thi trên UI thread
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await _viewModel.LoadSongsCommand.ExecuteAsync(null);
        });
    }
}