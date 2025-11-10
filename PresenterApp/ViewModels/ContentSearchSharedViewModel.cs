// File: ViewModels/ContentSearchSharedViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PresenterApp.Views;
using System.Linq;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    // Đây là ViewModel cho logic tìm kiếm có thể tái sử dụng
    public partial class ContentSearchSharedViewModel : BaseViewModel, IQueryAttributable
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
        bool isExactSearch = false;

        [ObservableProperty]
        ObservableCollection<Tag> filterTags = new();

        // --- Kết quả ---
        [ObservableProperty]
        ObservableCollection<BookSummaryViewModel> searchResults = new();

        [ObservableProperty]
        bool isBusySearching;

        // --- Thuộc tính điều khiển giao diện ---
        [ObservableProperty]
        bool showBookTypeFilter = true;

        [ObservableProperty]
        bool showBookFilter = true;

        [ObservableProperty]
        bool isSelectionMode = false;

        // Biến tạm để lưu ID bộ lọc được truyền vào
        private int _passedBookTypeId = 0;
        private int _passedBookId = 0;
        private int _passedTagId = 0;

        // --- Bối cảnh (nếu dùng trong EditBookPage) ---
        private Book? _specificBookContext = null;

        public ContentSearchSharedViewModel(DataAccessService dataAccess, FilterStateService filterStateService)
        {
            _dataAccess = dataAccess;
            _filterStateService = filterStateService;
            Title = "Lọc và Tìm kiếm";
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("IsSelectionMode", out var isSelectionMode))
            {
                IsSelectionMode = bool.Parse(isSelectionMode.ToString());
            }

            if (query.TryGetValue("BookTypeId", out var btId) && int.TryParse(btId.ToString(), out int parsedBtId))
            {
                _passedBookTypeId = parsedBtId;
            }

            if (query.TryGetValue("BookId", out var bId) && int.TryParse(bId.ToString(), out int parsedBId))
            {
                _passedBookId = parsedBId;
            }

            if (query.TryGetValue("TagId", out var tId) && int.TryParse(tId.ToString(), out int parsedTId))
            {
                _passedTagId = parsedTId;
            }

            // Các giá trị _passed...Id này sẽ được sử dụng bởi
            // phương thức LoadGeneralFiltersAsync()
        }

        public async Task InitializeAsync(Book? specificBookContext = null)
        {
            _specificBookContext = specificBookContext;

            if (_specificBookContext != null)
            {
                // Nếu được cung cấp sách cụ thể (từ EditBookPage)
                // Ẩn bộ lọc Sách và Loại Sách
                ShowBookTypeFilter = false;
                ShowBookFilter = false;
                // Tải thuộc tính cho riêng sách này
                _allAttributes = await _dataAccess.GetAllAttributesForBookAsync(_specificBookContext.Id);
                LoadAttributesForFilter();
            }
            else
            {
                // Nếu ở trang tìm kiếm chung
                ShowBookTypeFilter = true;
                ShowBookFilter = true;
                await LoadGeneralFiltersAsync();
            }

            // Tải Tags đã lọc từ dịch vụ
            FilterTags.Clear();
            foreach (var tag in _filterStateService.SelectedFilterTags)
            {
                FilterTags.Add(tag);
            }

            // Chạy tìm kiếm ban đầu
            await ExecuteSearchAsync();
        }

        // Tải bộ lọc chung (cho trang tìm kiếm chính)
        async Task LoadGeneralFiltersAsync()
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
                _allAttributes = _allAttributes.GroupBy(ad => ad.Name).Select(g => g.First()).ToList();

                // Áp dụng bộ lọc được truyền vào (nếu có)
                var bookTypeToSelect = allBookTypesOption;
                if (_passedBookTypeId > 0)
                {
                    bookTypeToSelect = filterBookTypes.FirstOrDefault(bt => bt.Id == _passedBookTypeId) ?? allBookTypesOption;
                }

                SelectedBookType = bookTypeToSelect;
                OnSelectedBookTypeChanged(bookTypeToSelect); // Tải danh sách sách

                // Áp dụng bộ lọc Sách (nếu có)
                if (_passedBookId > 0)
                {
                    // FilterBooks đã được nạp bởi OnSelectedBookTypeChanged
                    SelectedBook = FilterBooks.FirstOrDefault(b => b.Id == _passedBookId);
                }

                // (Bạn có thể thêm logic cho _passedTagId tại đây nếu cần)

                // Reset lại các ID đã qua
                _passedBookTypeId = 0;
                _passedBookId = 0;
                _passedTagId = 0;
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
            FilterBooks.Add(new Book { Id = 0, Name = "Tất cả Sách" });

            List<Book> booksToShow;
            if (value == null || value.Id == 0)
            {
                booksToShow = _allBooks;
            }
            else
            {
                booksToShow = _allBooks.Where(b => b.BookTypeId == value.Id).ToList();
            }

            foreach (var book in booksToShow) FilterBooks.Add(book);
            SelectedBook = FilterBooks.FirstOrDefault();
        }

        // Lọc Thuộc tính khi Sách thay đổi
        partial void OnSelectedBookChanged(Book? value)
        {
            LoadAttributesForFilter(value);
        }

        // Tải thuộc tính cho bộ lọc
        void LoadAttributesForFilter(Book? selectedBook = null)
        {
            FilterAttributes.Clear();
            FilterAttributes.Add(new AttributeDefinition { Id = 0, Name = "Tất cả Thuộc tính" });

            List<AttributeDefinition> attributesToShow;

            if (_specificBookContext != null)
            {
                // Dùng thuộc tính của sách trong bối cảnh
                attributesToShow = _allAttributes;
            }
            else if (selectedBook == null || selectedBook.Id == 0) // "Tất cả Sách"
            {
                if (SelectedBookType == null || SelectedBookType.Id == 0)
                {
                    attributesToShow = _allAttributes;
                }
                else
                {
                    var bookIdsInType = _allBooks.Where(b => b.BookTypeId == SelectedBookType.Id).Select(b => b.Id);
                    attributesToShow = _allAttributes.Where(ad =>
                        (ad.BookTypeId.HasValue && ad.BookTypeId.Value == SelectedBookType.Id) ||
                        (ad.BookId.HasValue && bookIdsInType.Contains(ad.BookId.Value))
                    ).ToList();
                }
            }
            else // Một sách cụ thể đã được chọn
            {
                var book = _allBooks.FirstOrDefault(b => b.Id == selectedBook.Id);
                if (book != null)
                {
                    attributesToShow = _allAttributes
                        .Where(ad => ad.BookTypeId == book.BookTypeId || ad.BookId == book.Id)
                        .ToList();
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
            SelectedAttribute = FilterAttributes.FirstOrDefault();
        }

        [RelayCommand]
        async Task OpenTagFilterAsync()
        {
            _filterStateService.SelectedFilterTags = FilterTags.ToList();
            await Shell.Current.GoToAsync(nameof(TagSelectionPage), true, new Dictionary<string, object>
            {
                { "Target", "Filter" },
                { "TargetId", 0 }
            });
        }

        [RelayCommand]
        public async Task ExecuteSearchAsync()
        {
            if (IsBusySearching) return;
            IsBusySearching = true;

            try
            {
                // Xác định ID sách và loại sách dựa trên bối cảnh
                int? bookId = _specificBookContext?.Id ?? ((SelectedBook?.Id == 0) ? null : SelectedBook?.Id);
                int? bookTypeId = _specificBookContext != null ? null : ((SelectedBookType?.Id == 0) ? null : SelectedBookType?.Id);
                int? attributeId = (SelectedAttribute?.Id == 0) ? null : SelectedAttribute?.Id;
                List<int> tagIds = FilterTags.Select(t => t.Id).ToList();

                var matchingEntries = await _dataAccess.SearchContentEntriesAsync(SearchText, IsExactSearch, bookTypeId, bookId, attributeId, tagIds);

                var entriesByBook = matchingEntries.GroupBy(entry => entry.BookId);
                SearchResults.Clear();

                // Tải lại _allBooks nếu cần (chỉ xảy ra ở trang tìm kiếm chung)
                if (_allBooks.Count == 0 && _specificBookContext == null)
                {
                    _allBooks = await _dataAccess.GetBooksAsync();
                }

                // Tạo dictionary sách cho bối cảnh này
                var bookDictionary = _specificBookContext != null
                    ? new Dictionary<int, Book> { { _specificBookContext.Id, _specificBookContext } }
                    : _allBooks.ToDictionary(b => b.Id, b => b);


                foreach (var group in entriesByBook)
                {
                    if (!bookDictionary.TryGetValue(group.Key, out var book)) continue;

                    var bookSummaryVM = new BookSummaryViewModel(book);

                    // Lấy thuộc tính cho sách này
                    List<AttributeDefinition> attributesForThisBook;
                    if (_specificBookContext != null)
                    {
                        attributesForThisBook = _allAttributes; // Đã tải trong InitializeAsync
                    }
                    else
                    {
                        // Tải nhanh nếu chưa có
                        var common = _allAttributes.Where(ad => ad.BookTypeId == book.BookTypeId);
                        var privateAttrs = _allAttributes.Where(ad => ad.BookId == book.Id);
                        attributesForThisBook = common.Concat(privateAttrs).ToList();
                    }

                    var loadTasks = new List<Task>();
                    foreach (var entry in group)
                    {
                        var contentVM = new ContentEntryViewModel(entry);
                        bookSummaryVM.ContentEntries.Add(contentVM);
                        loadTasks.Add(contentVM.LoadSummaryAsync(_dataAccess, attributesForThisBook));
                    }
                    await Task.WhenAll(loadTasks);

                    bookSummaryVM.UpdateResultCount();
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
        async Task SelectContent(ContentEntryViewModel contentEntryVM)
        {
            if (contentEntryVM == null) return;

            // Tạo tham số để trả về trang trước (CreatePresentationPage)
            var navParams = new ShellNavigationQueryParameters
            {
                { "SelectedContentEntryId", contentEntryVM.Entry.Id }
            };

            // Quay lại trang trước
            await Shell.Current.GoToAsync("..", navParams);
        }
    }
}