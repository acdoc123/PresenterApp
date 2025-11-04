// File: Views/EditTagPage.xaml.cs
using PresenterApp.ViewModels;
using PresenterApp.Models;

namespace PresenterApp.Views;

[QueryProperty(nameof(Item), "Item")]
public partial class EditTagPage : ContentPage
{
    public Tag Item
    {
        set => _viewModel.Initialize(value);
    }

    private readonly EditTagViewModel _viewModel;

    public EditTagPage(EditTagViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
}