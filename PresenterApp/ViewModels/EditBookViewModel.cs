// File: ViewModels/EditBookViewModel.cs
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
    public partial class EditBookViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        Book currentBook;

        [ObservableProperty]
        ObservableCollection<BookType> bookTypes = new();

        [ObservableProperty]
        ObservableCollection<Tag> selectedTags = new();

        [ObservableProperty]
        BookType selectedBookType;

        [ObservableProperty]
        ObservableCollection<AttributeDefinition> commonAttributes = new();

        [ObservableProperty]
        ObservableCollection<AttributeDefinition> privateAttributes = new();

        [ObservableProperty]
        string newAttributeName;

        [ObservableProperty]
        FieldType newAttributeType;

        public ObservableCollection<FieldType> FieldTypes { get; } = new(System.Enum.GetValues(typeof(FieldType)).Cast<FieldType>());

        // --- Logic Lọc/Tìm kiếm ---
        [ObservableProperty]
        string contentSearchText = string.Empty;

        // Danh sách đầy đủ
        private List<ContentEntryViewModel> _allContentEntries = new();

        // Danh sách đã lọc để hiển thị
        [ObservableProperty]
        ObservableCollection<ContentEntryViewModel> filteredContentEntries = new();

        [ObservableProperty]
        ObservableCollection<AttributeDefinition> filterAttributes = new();

        [ObservableProperty]
        AttributeDefinition selectedAttribute;

        [ObservableProperty]
        bool isExactSearch = false;

        // Biến tạm để lưu tất cả thuộc tính (dùng cho việc tải tóm tắt)
        private List<AttributeDefinition> _allAttributesForBook = new();

        [ObservableProperty]
        bool isEditingBookDetails;

        public EditBookViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public void Initialize(Book book)
        {
            CurrentBook = book;
            if (book.Id == 0)
            {
                Title = "Thêm Sách Mới";
                IsEditingBookDetails = true;
            }
            else
            {
                Title = CurrentBook.Name;
                IsEditingBookDetails = false;
            }
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
            //if (IsBusy) return;
            //IsBusy = true;
            try
            {
                BookTypes.Clear();
                var bts = await _dataAccess.GetBookTypesAsync();
                foreach (var bt in bts) BookTypes.Add(bt);

                SelectedTags.Clear();
                if (CurrentBook.Id != 0)
                {
                    var selectedBookTagsRelations = await _dataAccess.GetTagsForBookAsync(CurrentBook.Id);
                    if (selectedBookTagsRelations.Any())
                    {
                        var allTags = await _dataAccess.GetTagsAsync();
                        var selectedTagIds = selectedBookTagsRelations.Select(t => t.TagId).ToHashSet();
                        foreach (var tag in allTags.Where(t => selectedTagIds.Contains(t.Id)))
                        {
                            SelectedTags.Add(tag);
                        }
                    }
                }

                if (CurrentBook.Id != 0)
                {
                    SelectedBookType = BookTypes.FirstOrDefault(b => b.Id == CurrentBook.BookTypeId);
                    // Tải thuộc tính VÀO BỘ LỌC
                    await LoadAttributesForFilterAsync();
                    await ExecuteContentSearchCommand.ExecuteAsync(null);
                }
            }
            finally
            {
                //IsBusy = false;
            }
        }
        async Task LoadAttributesForFilterAsync()
        {
            await LoadPrivateAttributesAsync();

            _allAttributesForBook = CommonAttributes.Concat(PrivateAttributes).ToList();

            FilterAttributes.Clear();
            FilterAttributes.Add(new AttributeDefinition { Id = 0, Name = "Tất cả Thuộc tính" });
            foreach (var attr in _allAttributesForBook)
            {
                FilterAttributes.Add(attr);
            }
            SelectedAttribute = FilterAttributes.FirstOrDefault();
        }

        partial void OnSelectedBookTypeChanged(BookType value)
        {
            LoadCommonAttributesAsync();
        }

        async Task LoadCommonAttributesAsync()
        {
            CommonAttributes.Clear();
            if (SelectedBookType == null) return;

            var attributes = await _dataAccess.GetAttributesForBookTypeAsync(SelectedBookType.Id);
            foreach (var attr in attributes) CommonAttributes.Add(attr);
        }

        async Task LoadPrivateAttributesAsync()
        {
            PrivateAttributes.Clear();
            var attributes = await _dataAccess.GetAttributesForBookAsync(CurrentBook.Id);
            foreach (var attr in attributes) PrivateAttributes.Add(attr);
        }
        [RelayCommand]
        async Task ExecuteContentSearchAsync()
        {
            if (CurrentBook.Id == 0) return;
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                // 1. Lấy các giá trị bộ lọc
                int? attributeId = (SelectedAttribute?.Id == 0) ? null : SelectedAttribute?.Id;
                // (Bạn có thể thêm tagIds ở đây nếu muốn)
                List<int> tagIds = new List<int>();

                // 2. Gọi DataAccessService
                var matchingEntries = await _dataAccess.SearchContentEntriesAsync(
                    ContentSearchText,
                    IsExactSearch,
                    null,             // bookTypeId = null (đã ở trong sách)
                    CurrentBook.Id,   // bookId = ID sách hiện tại
                    attributeId,
                    tagIds
                );

                // 3. Chuyển đổi kết quả sang ViewModel
                FilteredContentEntries.Clear();

                // Đảm bảo _allAttributesForBook đã được tải
                if (!_allAttributesForBook.Any())
                {
                    await LoadAttributesForFilterAsync();
                }

                var loadTasks = new List<Task>();
                foreach (var entry in matchingEntries)
                {
                    var vm = new ContentEntryViewModel(entry);
                    loadTasks.Add(vm.LoadSummaryAsync(_dataAccess, _allAttributesForBook)); // Tải tóm tắt
                    FilteredContentEntries.Add(vm);
                }
                // Chờ tất cả tóm tắt tải xong
                await Task.WhenAll(loadTasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi tìm kiếm nội dung: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", "Không thể thực hiện tìm kiếm.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task AddPrivateAttributeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewAttributeName) || CurrentBook.Id == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng lưu Sách trước khi thêm thuộc tính riêng.", "OK");
                return;
            }
            var newAttr = new AttributeDefinition
            {
                Name = NewAttributeName,
                Type = NewAttributeType,
                BookId = CurrentBook.Id
            };
            await _dataAccess.SaveAttributeDefinitionAsync(newAttr);
            PrivateAttributes.Add(newAttr);
            NewAttributeName = string.Empty;
        }

        [RelayCommand]
        async Task DeleteAttributeAsync(AttributeDefinition attribute)
        {
            if (attribute == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận Xóa", $"Bạn có chắc muốn xóa thuộc tính '{attribute.Name}'?", "Có", "Không");
            if (!confirm) return;
            await _dataAccess.DeleteAttributeDefinitionAsync(attribute);
            PrivateAttributes.Remove(attribute);
        }

        [RelayCommand]
        async Task SaveBookAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentBook.Name) || SelectedBookType == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập Tên Sách và chọn Loại Sách.", "OK");
                return;
            }
            CurrentBook.BookTypeId = SelectedBookType.Id;
            await _dataAccess.SaveBookAsync(CurrentBook);
            CurrentBook.BookTypeId = SelectedBookType.Id;
            await _dataAccess.SaveBookAsync(CurrentBook);
            Title = CurrentBook.Name;
            IsEditingBookDetails = false;
            OnPropertyChanged(nameof(CurrentBook));
            await Shell.Current.DisplayAlert("Thành công", "Đã lưu Sách.", "OK");
        }

        [RelayCommand]
        async Task DeleteBookAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa '{CurrentBook.Name}'?", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeleteBookAsync(CurrentBook);
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        async Task OpenTagSelectionModalAsync()
        {
            if (CurrentBook.Id == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng lưu sách trước khi thêm tag.", "OK");
                return;
            }
            await Shell.Current.GoToAsync(nameof(TagSelectionPage), true, new Dictionary<string, object>
            {
                { "BookId", CurrentBook.Id }
            });
        }

        [RelayCommand]
        async Task RemoveTagFromBookAsync(Tag tagToRemove)
        {
            if (tagToRemove == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Xóa tag '{tagToRemove.Name}' khỏi sách này?", "Có", "Không");
            if (!confirm) return;
            SelectedTags.Remove(tagToRemove);
            await _dataAccess.SetTagsForBookAsync(CurrentBook.Id, SelectedTags.ToList());
        }

        [RelayCommand]
        void ToggleEditMode()
        {
            IsEditingBookDetails = !IsEditingBookDetails;
            if (IsEditingBookDetails)
                Title = $"Sửa: {CurrentBook.Name}";
            else
                Title = CurrentBook.Name;
        }

        [RelayCommand]
        async Task AddContentEntryAsync()
        {
            await Shell.Current.GoToAsync(nameof(EditContentEntryPage), true, new Dictionary<string, object>
            {
                { "Book", CurrentBook },
                { "Entry", new ContentEntry { BookId = CurrentBook.Id } }
            });
        }

        [RelayCommand]
        async Task EditContentEntryAsync(ContentEntryViewModel entryVM)
        {
            if (entryVM == null) return;
            await Shell.Current.GoToAsync(nameof(EditContentEntryPage), true, new Dictionary<string, object>
            {
                { "Book", CurrentBook },
                { "Entry", entryVM.Entry }
            });
        }

        [RelayCommand]
        async Task DeleteContentEntryAsync(ContentEntryViewModel entryVM)
        {
            if (entryVM == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa nội dung này?", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeleteContentEntryAsync(entryVM.Entry);
                _allContentEntries.Remove(entryVM); // Xóa khỏi danh sách master
                await ExecuteContentSearchAsync(); // Lọc lại danh sách hiển thị
            }
        }
    }
}