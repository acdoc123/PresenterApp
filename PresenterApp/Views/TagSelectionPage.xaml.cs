// File: Views/TagSelectionPage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp.Views;

[QueryProperty(nameof(BookId), "BookId")]
public partial class TagSelectionPage : ContentPage
{
    public int BookId
    {
        set => _viewModel.Initialize(value);
    }

    private readonly TagSelectionViewModel _viewModel;

    public TagSelectionPage(TagSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadTagsCommand.Execute(null);
    }
}