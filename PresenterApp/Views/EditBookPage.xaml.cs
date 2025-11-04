// File: Views/EditBookPage.xaml.cs
using PresenterApp.ViewModels;
using PresenterApp.Models;

namespace PresenterApp.Views;

[QueryProperty(nameof(Item), "Item")]
public partial class EditBookPage : ContentPage
{
    public Book Item
    {
        set => _viewModel.Initialize(value);
    }

    private readonly EditBookViewModel _viewModel;

    public EditBookPage(EditBookViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadDataCommand.Execute(null);
    }
}