// File: ViewModels/HomeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using PresenterApp.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        // --- Danh sách Master cho các bộ lọc ---
        private List<BookType> _allBookTypes = new();
        private List<Book> _allBooks = new();
        private List<AttributeDefinition> _allAttributes = new();

        // --- Thuộc tính UI cho Bộ lọc ---
        [ObservableProperty]
        ObservableCollection<BookType> filterBookTypes = new();

        [ObservableProperty]
        BookType? selectedBookType;

        [ObservableProperty]
        ObservableCollection<Book> filterBooks = new();

        [ObservableProperty]
        Book? selectedBook;

        [ObservableProperty]
        ObservableCollection<AttributeDefinition> filterAttributes = new();

        [ObservableProperty]
        AttributeDefinition? selectedAttribute;

        [ObservableProperty]
        string searchText = string.Empty;

        // --- Kết quả ---
        [ObservableProperty]
        ObservableCollection<BookSummaryViewModel> searchResults = new();

        [ObservableProperty]
        bool isBusySearching;

        public HomeViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            Title = "Trang chủ";
        }

        // Tải dữ liệu cho các bộ lọc
        [RelayCommand]
        public async Task LoadFiltersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // Tải tất cả Loại Sách
                _allBookTypes = await _dataAccess.GetBookTypesAsync();
                FilterBookTypes.Clear();
                FilterBookTypes.Add(new BookType { Id = 0, Name = "Tất cả Loại Sách" }); // Thêm mục "Tất cả"
                foreach (var bt in _allBookTypes) FilterBookTypes.Add(bt);

                // Tải tất cả Sách
                _allBooks = await _dataAccess.GetBooksAsync();

                // Tải tất cả Thuộc tính (chỉ để tham khảo, sẽ lọc sau)
                _allAttributes.Clear();
                foreach (var book in _allBooks)
                {
                    var attrs = await _dataAccess.GetAllAttributesForBookAsync(book.Id);
                    _allAttributes.AddRange(attrs);
                }
                // Loại bỏ các thuộc tính trùng tên
                _allAttributes = _allAttributes.GroupBy(ad => ad.Name).Select(g => g.First()).ToList();

                // Đặt lại các bộ lọc
                OnSelectedBookTypeChanged(null);

                // Chạy tìm kiếm mặc định (tất cả nội dung)
                await ExecuteSearchAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Lọc Sách khi Loại Sách thay đổi
        partial void OnSelectedBookTypeChanged(BookType? value)
        {
            FilterBooks.Clear();
            FilterBooks.Add(new Book { Id = 0, Name = "Tất cả Sách" }); // Thêm mục "Tất cả"

            List<Book> booksToShow;
            if (value == null || value.Id == 0) // "Tất cả Loại Sách"
            {
                booksToShow = _allBooks;
            }
            else
            {
                booksToShow = _allBooks.Where(b => b.BookTypeId == value.Id).ToList();
            }

            foreach (var book in booksToShow) FilterBooks.Add(book);
            SelectedBook = FilterBooks.FirstOrDefault(); // Đặt lại về "Tất cả Sách"
        }

        // Lọc Thuộc tính khi Sách thay đổi
        partial void OnSelectedBookChanged(Book? value)
        {
            FilterAttributes.Clear();
            FilterAttributes.Add(new AttributeDefinition { Id = 0, Name = "Tất cả Thuộc tính" }); // Thêm mục "Tất cả"

            List<AttributeDefinition> attributesToShow;
            if (value == null || value.Id == 0) // "Tất cả Sách"
            {
                // Nếu "Loại Sách" cũng là "Tất cả", hiển thị tất cả thuộc tính
                if (SelectedBookType == null || SelectedBookType.Id == 0)
                {
                    attributesToShow = _allAttributes;
                }
                else // Hiển thị thuộc tính của "Loại Sách" đã chọn
                {
                    attributesToShow = _allAttributes.Where(ad => ad.BookTypeId == SelectedBookType.Id).ToList();
                }
            }
            else // Một sách cụ thể đã được chọn
            {
                // Lấy thuộc tính của Sách đó (chung + riêng)
                // (Sử dụng danh sách đã tải trước để tăng tốc)
                var commonIds = _allAttributes
                    .Where(ad => ad.BookTypeId == value.BookTypeId)
                    .Select(ad => ad.Id);
                var privateIds = _allAttributes
                    .Where(ad => ad.BookId == value.Id)
                    .Select(ad => ad.Id);

                var allIds = commonIds.Concat(privateIds).ToHashSet();
                attributesToShow = _allAttributes.Where(ad => allIds.Contains(ad.Id)).ToList();
            }

            foreach (var attr in attributesToShow.GroupBy(ad => ad.Name).Select(g => g.First()))
            {
                FilterAttributes.Add(attr);
            }
            SelectedAttribute = FilterAttributes.FirstOrDefault(); // Đặt lại về "Tất cả Thuộc tính"
        }

        // Lệnh được gọi bởi SearchBar hoặc nút
        [RelayCommand]
        async Task ExecuteSearchAsync()
        {
            if (IsBusySearching) return;
            IsBusySearching = true;

            try
            {
                // Lấy ID nullable từ các bộ lọc
                int? bookTypeId = (SelectedBookType?.Id == 0) ? null : SelectedBookType?.Id;
                int? bookId = (SelectedBook?.Id == 0) ? null : SelectedBook?.Id;
                int? attributeId = (SelectedAttribute?.Id == 0) ? null : SelectedAttribute?.Id;

                // 1. Tìm kiếm trong DB
                var matchingEntries = await _dataAccess.SearchContentEntriesAsync(SearchText, bookTypeId, bookId, attributeId);

                // 2. Nhóm kết quả theo Sách
                var entriesByBook = matchingEntries.GroupBy(entry => entry.BookId);

                SearchResults.Clear();

                // Lấy các thuộc tính để hiển thị tóm tắt
                var allAttributesMap = (await _dataAccess.GetBooksAsync())
                    .ToDictionary(b => b.Id, b => _dataAccess.GetAllAttributesForBookAsync(b.Id));

                foreach (var group in entriesByBook)
                {
                    var book = _allBooks.FirstOrDefault(b => b.Id == group.Key);
                    if (book == null) continue;

                    var bookSummaryVM = new BookSummaryViewModel(book);

                    // Lấy định nghĩa thuộc tính cho sách này
                    if (!allAttributesMap.ContainsKey(book.Id)) continue;
                    var attributesForThisBook = await allAttributesMap[book.Id];

                    // 3. Tải ContentEntryViewModel với tóm tắt
                    var loadTasks = new List<Task>();
                    foreach (var entry in group)
                    {
                        var contentVM = new ContentEntryViewModel(entry);
                        bookSummaryVM.ContentEntries.Add(contentVM);
                        // Tải tóm tắt (2 thuộc tính đầu)
                        loadTasks.Add(contentVM.LoadSummaryAsync(_dataAccess, attributesForThisBook));
                    }
                    await Task.WhenAll(loadTasks);

                    bookSummaryVM.UpdateResultCount(); // Cập nhật đếm "x nội dung"
                    SearchResults.Add(bookSummaryVM);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi tìm kiếm: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", "Không thể thực hiện tìm kiếm.", "OK");
            }
            finally
            {
                IsBusySearching = false;
            }
        }

        [RelayCommand]
        async Task GoToManagementAsync()
        {
            await Shell.Current.GoToAsync(nameof(ManagementDashboardPage));
        }
    }
}