// File: Views/SongDetailPage.xaml.cs
using PresenterApp.ViewModels;
using System.Globalization;

namespace PresenterApp.Views;

public partial class SongDetailPage : ContentPage
{
    private readonly SongDetailViewModel _viewModel;
    public SongDetailPage(SongDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }
}

// Converter để ẩn nút "Xóa" khi thêm bài hát mới
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int intValue && intValue > 0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}