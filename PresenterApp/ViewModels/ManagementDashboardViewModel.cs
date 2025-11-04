// File: ViewModels/ManagementDashboardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using PresenterApp.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace PresenterApp.ViewModels
{
    public partial class ManagementDashboardViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        ObservableCollection<BookType> bookTypes = new();

        [ObservableProperty]
        ObservableCollection<Book> books = new();

        [ObservableProperty]
        ObservableCollection<Tag> tags = new();

        public ManagementDashboardViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            Title = "Quản lý";
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                BookTypes.Clear();
                var bts = await _dataAccess.GetBookTypesAsync();
                foreach (var bt in bts) BookTypes.Add(bt);

                Books.Clear();
                var bks = await _dataAccess.GetBooksAsync();
                foreach (var bk in bks) Books.Add(bk);

                Tags.Clear();
                var tgs = await _dataAccess.GetTagsAsync();
                foreach (var tg in tgs) Tags.Add(tg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi không tải được dữ liệu: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", "Không thể tải dữ liệu", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task EditItemAsync(object item)
        {
            // Điều hướng đến trang chỉnh sửa tương ứng
            if (item is BookType bt)
            {
                await Shell.Current.GoToAsync(nameof(EditBookTypePage), true, new Dictionary<string, object>
                {
                    { "Item", bt }
                });
            }
            else if (item is Book b)
            {
                await Shell.Current.GoToAsync(nameof(EditBookPage), true, new Dictionary<string, object>
                {
                    { "Item", b }
                });
            }
            // Tương tự cho Tag...
        }

        [RelayCommand]
        async Task AddNewAsync(string itemType)
        {
            // Điều hướng đến trang chỉnh sửa với một đối tượng mới
            if (itemType == "BookType")
            {
                await Shell.Current.GoToAsync(nameof(EditBookTypePage), true, new Dictionary<string, object>
                {
                    { "Item", new BookType() }
                });
            }
            else if (itemType == "Book")
            {
                await Shell.Current.GoToAsync(nameof(EditBookPage), true, new Dictionary<string, object>
                {
                    { "Item", new Book() }
                });
            }
            // Tương tự cho Tag...
        }
    }
}