// File: MainPage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp
{
    public partial class MainPage : ContentPage
    {
        private readonly HomeViewModel _viewModel;
        public MainPage(HomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Tải (hoặc tải lại) danh sách sách mỗi khi quay lại trang chủ
            _viewModel.LoadBooksCommand.ExecuteAsync(null);
        }
    }
}