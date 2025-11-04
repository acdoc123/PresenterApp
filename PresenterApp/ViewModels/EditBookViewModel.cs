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

        // --- Danh sách cho Picker ---
        [ObservableProperty]
        ObservableCollection<BookType> bookTypes = new();

        // --- THAY ĐỔI TỪ ĐÂY ---
        [ObservableProperty]
        ObservableCollection<SelectableTag> selectableTags = new();
        // --- KẾT THÚC THAY ĐỔI ---

        // --- Mục đã chọn ---
        [ObservableProperty]
        BookType selectedBookType;

        // --- Thuộc tính ---
        [ObservableProperty]
        ObservableCollection<AttributeDefinition> commonAttributes = new();

        [ObservableProperty]
        ObservableCollection<AttributeDefinition> privateAttributes = new();

        [ObservableProperty]
        string newAttributeName;

        [ObservableProperty]
        FieldType newAttributeType;

        public ObservableCollection<FieldType> FieldTypes { get; } = new(System.Enum.GetValues(typeof(FieldType)).Cast<FieldType>());

        // --- Nội dung ---
        [ObservableProperty]
        ObservableCollection<ContentEntry> contentEntries = new();

        public EditBookViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public void Initialize(Book book)
        {
            CurrentBook = book;
            Title = book.Id == 0 ? "Thêm Sách Mới" : $"Sửa: {book.Name}";
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // Tải danh sách Loại Sách
                BookTypes.Clear();
                var bts = await _dataAccess.GetBookTypesAsync();
                foreach (var bt in bts) BookTypes.Add(bt);

                // Tải tất cả Tags và các Tag đã chọn
                var allTags = await _dataAccess.GetTagsAsync();
                var selectedTagIds = new HashSet<int>();
                if (CurrentBook.Id != 0)
                {
                    var selectedBookTags = await _dataAccess.GetTagsForBookAsync(CurrentBook.Id);
                    selectedTagIds = selectedBookTags.Select(t => t.TagId).ToHashSet();
                }

                SelectableTags.Clear();
                foreach (var tag in allTags)
                {
                    SelectableTags.Add(new SelectableTag
                    {
                        Tag = tag,
                        IsSelected = selectedTagIds.Contains(tag.Id)
                    });
                }

                if (CurrentBook.Id != 0)
                {
                    // Tải BookType đã chọn
                    SelectedBookType = BookTypes.FirstOrDefault(b => b.Id == CurrentBook.BookTypeId);

                    // Tải thuộc tính riêng
                    await LoadPrivateAttributesAsync();

                    // Tải danh sách nội dung
                    await LoadContentEntriesAsync();
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- Xử lý Thuộc tính ---
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
            await _dataAccess.DeleteAttributeDefinitionAsync(attribute);
            PrivateAttributes.Remove(attribute);
        }

        // --- Xử lý Sách ---
        [RelayCommand]
        async Task SaveBookAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentBook.Name) || SelectedBookType == null)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập Tên Sách và chọn Loại Sách.", "OK");
                return;
            }

            CurrentBook.BookTypeId = SelectedBookType.Id;
            await _dataAccess.SaveBookAsync(CurrentBook); // Lưu sách để lấy ID (nếu là sách mới)

            // Lưu các Tag đã chọn
            var tagsToSave = SelectableTags.Where(t => t.IsSelected).Select(t => t.Tag).ToList();
            await _dataAccess.SetTagsForBookAsync(CurrentBook.Id, tagsToSave);

            Title = $"Sửa: {CurrentBook.Name}";
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

        // --- Xử lý Nội dung ---
        async Task LoadContentEntriesAsync()
        {
            ContentEntries.Clear();
            var entries = await _dataAccess.GetContentEntriesAsync(CurrentBook.Id);
            foreach (var entry in entries) ContentEntries.Add(entry);
        }

        [RelayCommand]
        async Task AddContentEntryAsync()
        {
            // Điều hướng đến trang thêm nội dung
            await Shell.Current.GoToAsync(nameof(EditContentEntryPage), true, new Dictionary<string, object>
            {
                { "Book", CurrentBook },
                { "Entry", new ContentEntry { BookId = CurrentBook.Id } } // Gửi một entry mới
            });
        }

        [RelayCommand]
        async Task EditContentEntryAsync(ContentEntry entry)
        {
            // Điều hướng đến trang sửa nội dung
            await Shell.Current.GoToAsync(nameof(EditContentEntryPage), true, new Dictionary<string, object>
            {
                { "Book", CurrentBook }, // Gửi Sách (để biết các thuộc tính)
                { "Entry", entry } // Gửi Entry (để biết các giá trị)
            });
                  }

        [RelayCommand]
        async Task DeleteContentEntryAsync(ContentEntry entry)
        {
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Xóa nội dung này?", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeleteContentEntryAsync(entry);
                ContentEntries.Remove(entry);
            }
        }
    }
}