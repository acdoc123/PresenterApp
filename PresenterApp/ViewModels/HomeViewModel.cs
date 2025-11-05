// File: ViewModels/HomeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using PresenterApp.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        // Danh sách master
        private List<BookSummaryViewModel> _allBooks = new();

        // Danh sách hiển thị đã lọc
        [ObservableProperty]
        ObservableCollection<BookSummaryViewModel> bookList = new();

        [ObservableProperty]
        string searchText = string.Empty;

        public HomeViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            Title = "Trang chủ";
        }

        [RelayCommand]
        async Task LoadBooksAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var booksFromDb = await _dataAccess.GetBooksAsync();
                _allBooks.Clear();
                foreach (var book in booksFromDb)
                {
                    _allBooks.Add(new BookSummaryViewModel(book, _dataAccess));
                }
                FilterBooks();
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterBooks();
        }

        private void FilterBooks()
        {
            // TODO: Mở rộng logic tìm kiếm này để tìm kiếm cả nội dung (nếu đã tải)
            var filtered = _allBooks
                .Where(vm => string.IsNullOrWhiteSpace(SearchText)
                             || (vm.Book.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            BookList.Clear();
            foreach (var vm in filtered)
            {
                BookList.Add(vm);
            }
        }

        [RelayCommand]
        async Task GoToManagementAsync()
        {
            // Nút "Thêm sửa sách"
            await Shell.Current.GoToAsync(nameof(ManagementDashboardPage));
        }
    }
}