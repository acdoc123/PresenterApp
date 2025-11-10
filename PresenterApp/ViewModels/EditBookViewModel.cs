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
        private readonly FilterStateService _filterStateService; // Cần cho VM con

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

        [ObservableProperty]
        ContentSearchSharedViewModel contentSearchVM;

        [ObservableProperty]
        bool isEditingBookDetails;

        public EditBookViewModel(DataAccessService dataAccess, FilterStateService filterStateService)
        {
            _dataAccess = dataAccess;
            _filterStateService = filterStateService; // Lưu lại

            ContentSearchVM = new ContentSearchSharedViewModel(_dataAccess, _filterStateService);
            ContentSearchVM.ShowBookTypeFilter = false;
            ContentSearchVM.ShowBookFilter = false;
        }

        public async void Initialize(Book book)
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
            // Cấu hình VM con với bối cảnh là sách này
            await ContentSearchVM.InitializeAsync(CurrentBook);
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
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
                    // Tải thuộc tính (cho phần chỉnh sửa thuộc tính riêng)
                    await LoadPrivateAttributesAsync();
                    // Tải lại nội dung bằng VM con
                    await ContentSearchVM.ExecuteSearchAsync();
                }
            }
            finally
            {
                //IsBusy = false;
            }
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
                // Tải lại danh sách bằng cách gọi lệnh của VM con
                await ContentSearchVM.ExecuteSearchAsync();
            }
        }
    }
}