// File: Views/EditContentEntryPage.xaml.cs
using PresenterApp.ViewModels;
using PresenterApp.Models;

namespace PresenterApp.Views;

[QueryProperty(nameof(Book), "Book")]
[QueryProperty(nameof(Entry), "Entry")]
public partial class EditContentEntryPage : ContentPage
{
    public Book Book { set => _viewModel.Book = value; }
    public ContentEntry Entry { set => _viewModel.Initialize(value); }

    private readonly EditContentEntryViewModel _viewModel;

    public EditContentEntryPage(EditContentEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }
}