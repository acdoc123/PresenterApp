// File: Views/EditThemePage.xaml.cs
using PresenterApp.ViewModels;
using PresenterApp.Models;

namespace PresenterApp.Views;

[QueryProperty(nameof(Item), "Item")]
public partial class EditThemePage : ContentPage
{
    public PresentationTheme Item
    {
        set => _viewModel.Initialize(value);
    }

    private readonly EditThemeViewModel _viewModel;

    public EditThemePage(EditThemeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
}