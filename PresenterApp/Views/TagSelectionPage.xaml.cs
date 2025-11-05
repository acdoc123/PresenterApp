// File: Views/TagSelectionPage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp.Views;

// --- CẬP NHẬT QueryProperty ---
[QueryProperty(nameof(Target), "Target")]
[QueryProperty(nameof(TargetId), "TargetId")]
public partial class TagSelectionPage : ContentPage
{
    // Target: "Book", "Entry", hoặc "Filter"
    public string Target
    {
        set => _viewModel.Target = value;
    }

    // ID của Book hoặc Entry (hoặc 0 nếu là Filter)
    public int TargetId
    {
        set => _viewModel.TargetId = value;
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