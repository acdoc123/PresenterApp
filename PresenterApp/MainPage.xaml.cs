// File: MainPage.xaml.cs
using PresenterApp.ViewModels;

namespace PresenterApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel) // Inject MainViewModel
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // Xóa code OnCounterClicked cũ
    }
}