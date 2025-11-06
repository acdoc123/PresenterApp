// File: Views/EditStructurePage.xaml.cs
using PresenterApp.ViewModels;
using PresenterApp.Models;

namespace PresenterApp.Views;

[QueryProperty(nameof(Item), "Item")]
public partial class EditStructurePage : ContentPage
{
    public PresentationStructure Item
    {
        set => _viewModel.Initialize(value);
    }

    private readonly EditStructureViewModel _viewModel;

    public EditStructurePage(EditStructureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
}