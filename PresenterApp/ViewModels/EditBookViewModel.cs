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

        [ObservableProperty]
        ObservableCollection<Tag> selectedTags = new();

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
        ObservableCollection<ContentEntryViewModel> contentEntries = new();

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
                // Sách Mới: Luôn ở chế độ sửa
                Title = "Thêm Sách Mới";
                IsEditingBookDetails = true;
            }
            else
            {
                // Sách Cũ: Bắt đầu ở chế độ chỉ xem nội dung
                Title = CurrentBook.Name; // Chỉ hiển thị tên sách
                IsEditingBookDetails = false;
            }
        }

        [RelayCommand]
        void ToggleEditMode()
        {
            IsEditingBookDetails = !IsEditingBookDetails;

            // Cập nhật tiêu đề trang cho phù hợp
            if (IsEditingBookDetails)
                Title = $"Sửa: {CurrentBook.Name}";
            else
                Title = CurrentBook.Name;
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

                // Tải các Tag đã chọn
                SelectedTags.Clear();
                if (CurrentBook.Id != 0)
                {
                    // Lấy các bảng quan hệ
                    var selectedBookTagsRelations = await _dataAccess.GetTagsForBookAsync(CurrentBook.Id);
                    if (selectedBookTagsRelations.Any())
                    {
                        // Lấy thông tin Tag đầy đủ
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
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận Xóa", $"Bạn có chắc muốn xóa thuộc tính '{attribute.Name}'?", "Có", "Không");
            if (!confirm) return;

            await _dataAccess.DeleteAttributeDefinitionAsync(attribute);
            PrivateAttributes.Remove(attribute);
        }
        // --- Lệnh mở Modal/Trang chọn Tag ---
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

        //Lệnh xóa Tag khỏi Sách (nút 'x') ---
        [RelayCommand]
        async Task RemoveTagFromBookAsync(Tag tagToRemove)
        {
            if (tagToRemove == null) return;

            // Thêm xác nhận
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Xóa tag '{tagToRemove.Name}' khỏi sách này?", "Có", "Không");
            if (!confirm) return;

            SelectedTags.Remove(tagToRemove);
            // Lưu thay đổi ngay lập tức
            await _dataAccess.SetTagsForBookAsync(CurrentBook.Id, SelectedTags.ToList());
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
            await _dataAccess.SaveBookAsync(CurrentBook);

            // Lưu các Tag đã chọn
            var tagsToSave = SelectedTags.ToList();
            await _dataAccess.SetTagsForBookAsync(CurrentBook.Id, tagsToSave);

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

        // --- Xử lý Nội dung ---
        async Task LoadContentEntriesAsync()
        {
            ContentEntries.Clear();
            var commonAttrs = await _dataAccess.GetAttributesForBookTypeAsync(CurrentBook.BookTypeId);
            var privateAttrs = await _dataAccess.GetAttributesForBookAsync(CurrentBook.Id);
            var firstAttribute = commonAttrs.Concat(privateAttrs).FirstOrDefault();

            var entries = await _dataAccess.GetContentEntriesAsync(CurrentBook.Id);
            foreach (var entry in entries)
            {
                // Tạo ViewModel và tải tóm tắt
                var vm = new ContentEntryViewModel(entry);
                ContentEntries.Add(vm);
                // Tải tóm tắt (không cần chờ)
                _ = vm.LoadSummaryAsync(_dataAccess, firstAttribute);
            }
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
                { "Entry", entryVM.Entry } // Truyền Entry gốc
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
                ContentEntries.Remove(entryVM);
            }
        }
    }
}