// File: Views/EditBookTypePage.xaml.cs
using PresenterApp.ViewModels;
using PresenterApp.Models;
namespace PresenterApp.Views;

[QueryProperty(nameof(Item), "Item")]
public partial class EditBookTypePage : ContentPage
{
    public BookType Item
    {
        set => _viewModel.Initialize(value);
    }

    private readonly EditBookTypeViewModel _viewModel;

    public EditBookTypePage(EditBookTypeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
}