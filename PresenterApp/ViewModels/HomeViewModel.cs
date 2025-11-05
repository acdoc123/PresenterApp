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
        private readonly FilterStateService _filterStateService;

        // --- Danh sách Master cho các bộ lọc ---
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

        [ObservableProperty]
        bool isExactSearch = false; // Mặc định là tìm kiếm KHÔNG chính xác (fuzzy)
        [ObservableProperty]
        ObservableCollection<Tag> filterTags = new();

        // --- Kết quả ---
        [ObservableProperty]
        ObservableCollection<BookSummaryViewModel> searchResults = new();

        [ObservableProperty]
        bool isBusySearching;

        public HomeViewModel(DataAccessService dataAccess, FilterStateService filterStateService)
        {
            _dataAccess = dataAccess;
            _filterStateService = filterStateService;
            Title = "Trang chủ";
        }
        [RelayCommand]
        async Task OpenTagFilterAsync()
        {
            // Tải các tag đã chọn hiện tại vào dịch vụ
            _filterStateService.SelectedFilterTags = FilterTags.ToList();

            // Mở trang chọn Tag với bối cảnh "Filter"
            await Shell.Current.GoToAsync(nameof(TagSelectionPage), true, new Dictionary<string, object>
            {
                { "Target", "Filter" },
                { "TargetId", 0 } // Không cần ID
            });
        }

        // Tải dữ liệu cho các bộ lọc
        [RelayCommand]
        public async Task LoadFiltersAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var bookTypes = await _dataAccess.GetBookTypesAsync();
                filterBookTypes.Clear();
                var allBookTypesOption = new BookType { Id = 0, Name = "Tất cả Loại Sách" };
                filterBookTypes.Add(allBookTypesOption);
                foreach (var bt in bookTypes) filterBookTypes.Add(bt);

                _allBooks = (await _dataAccess.GetBooksAsync()).ToList();

                _allAttributes.Clear();
                foreach (var book in _allBooks)
                {
                    var attrs = await _dataAccess.GetAllAttributesForBookAsync(book.Id);
                    _allAttributes.AddRange(attrs);
                }
                FilterTags.Clear();
                foreach (var tag in _filterStateService.SelectedFilterTags)
                {
                    FilterTags.Add(tag);
                }
                // Loại bỏ các thuộc tính trùng tên
                _allAttributes = _allAttributes.GroupBy(ad => ad.Name).Select(g => g.First()).ToList();

                // Đặt lại các bộ lọc
                //OnSelectedBookTypeChanged(null);
                SelectedBookType = allBookTypesOption;

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
                    // Lấy ID của tất cả các sách thuộc loại này
                    var bookIdsInType = _allBooks.Where(b => b.BookTypeId == SelectedBookType.Id).Select(b => b.Id);
                    attributesToShow = _allAttributes.Where(ad =>
                        (ad.BookTypeId.HasValue && ad.BookTypeId.Value == SelectedBookType.Id) ||
                        (ad.BookId.HasValue && bookIdsInType.Contains(ad.BookId.Value))
                    ).ToList();
                }
            }
            else // Một sách cụ thể đã được chọn
            {
                // Lấy thuộc tính của Sách đó (chung + riêng)
                var book = _allBooks.FirstOrDefault(b => b.Id == value.Id);
                if (book != null)
                {
                    var commonIds = _allAttributes
                        .Where(ad => ad.BookTypeId == book.BookTypeId)
                        .Select(ad => ad.Id);
                    var privateIds = _allAttributes
                        .Where(ad => ad.BookId == book.Id)
                        .Select(ad => ad.Id);

                    var allIds = commonIds.Concat(privateIds).ToHashSet();
                    attributesToShow = _allAttributes.Where(ad => allIds.Contains(ad.Id)).ToList();
                }
                else
                {
                    attributesToShow = new List<AttributeDefinition>();
                }
            }

            foreach (var attr in attributesToShow.GroupBy(ad => ad.Name).Select(g => g.First()))
            {
                FilterAttributes.Add(attr);
            }
            SelectedAttribute = FilterAttributes.FirstOrDefault(); // Đặt lại về "Tất cả Thuộc tính"
        }

        // Lệnh được gọi bởi SearchBar
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

                List<int> tagIds = FilterTags.Select(t => t.Id).ToList();
                // --- Truyền IsExactSearch vào service ---
                var matchingEntries = await _dataAccess.SearchContentEntriesAsync(SearchText, IsExactSearch, bookTypeId, bookId, attributeId, tagIds);

                // 2. Nhóm kết quả theo Sách
                var entriesByBook = matchingEntries.GroupBy(entry => entry.BookId);
                SearchResults.Clear();

                // Lấy các thuộc tính để hiển thị tóm tắt
                // Tạo một Dictionary các Sách để tra cứu nhanh
                var bookDictionary = _allBooks.ToDictionary(b => b.Id, b => b);

                foreach (var group in entriesByBook)
                {
                    // Lấy sách từ Dictionary
                    if (!bookDictionary.TryGetValue(group.Key, out var book)) continue;

                    var bookSummaryVM = new BookSummaryViewModel(book);

                    // Lấy định nghĩa thuộc tính cho sách này (đã được cache)
                    var commonAttrs = _allAttributes.Where(ad => ad.BookTypeId == book.BookTypeId);
                    var privateAttrs = _allAttributes.Where(ad => ad.BookId == book.Id);
                    var attributesForThisBook = commonAttrs.Concat(privateAttrs).ToList();

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
            // Nút "Thêm sửa sách"
            await Shell.Current.GoToAsync(nameof(ManagementDashboardPage));
        }
    }
}