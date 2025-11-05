// File: ViewModels/EditContentEntryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class EditContentEntryViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        Book book;

        [ObservableProperty]
        ContentEntry currentEntry;

        [ObservableProperty]
        ObservableCollection<DynamicAttributeViewModel> dynamicAttributes = new();

        [ObservableProperty]
        ObservableCollection<Tag> selectedTags = new();

        public EditContentEntryViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public void Initialize(ContentEntry entry)
        {
            CurrentEntry = entry;
            Title = entry.Id == 0 ? "Thêm Nội dung Mới" : "Sửa Nội dung";
            // Book sẽ được set qua QueryProperty
            if (Book != null)
            {
                LoadAttributesAndValuesCommand.Execute(null);
            }
            LoadTagsCommand.Execute(null);
        }

        partial void OnBookChanged(Book value)
        {
            // Đảm bảo tải dữ liệu nếu Book được set sau
            if (CurrentEntry != null)
            {
                LoadAttributesAndValuesCommand.Execute(null);
            }
        }
        [RelayCommand]
        async Task LoadTagsAsync()
        {
            if (CurrentEntry.Id == 0) return; // Chỉ tải nếu Entry đã tồn tại

            SelectedTags.Clear();
            var tagRelations = await _dataAccess.GetTagsForContentEntryAsync(CurrentEntry.Id);
            if (tagRelations.Any())
            {
                var allTags = await _dataAccess.GetTagsAsync();
                var selectedTagIds = tagRelations.Select(t => t.TagId).ToHashSet();
                foreach (var tag in allTags.Where(t => selectedTagIds.Contains(t.Id)))
                {
                    SelectedTags.Add(tag);
                }
            }
        }

        [RelayCommand]
        async Task LoadAttributesAndValuesAsync()
        {
            if (Book == null || CurrentEntry == null) return;
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                DynamicAttributes.Clear();

                // 1. Lấy tất cả các định nghĩa thuộc tính (chung + riêng)
                var commonAttrs = await _dataAccess.GetAttributesForBookTypeAsync(Book.BookTypeId);
                var privateAttrs = await _dataAccess.GetAttributesForBookAsync(Book.Id);
                var allDefinitions = commonAttrs.Concat(privateAttrs).ToList();

                // 2. Lấy các giá trị đã lưu (nếu là sửa)
                var allValues = new Dictionary<int, AttributeValue>();
                if (CurrentEntry.Id != 0)
                {
                    var values = await _dataAccess.GetAttributeValuesAsync(CurrentEntry.Id);
                    allValues = values.ToDictionary(v => v.AttributeDefinitionId, v => v);
                }

                // 3. Tạo các ViewModel động
                foreach (var def in allDefinitions)
                {
                    AttributeValue value;
                    if (!allValues.TryGetValue(def.Id, out value))
                    {
                        // Tạo một giá trị mới nếu nó chưa tồn tại
                        value = new AttributeValue
                        {
                            ContentEntryId = CurrentEntry.Id, // Sẽ là 0 nếu Entry là mới
                            AttributeDefinitionId = def.Id,
                            Value = string.Empty
                        };
                    }
                    DynamicAttributes.Add(new DynamicAttributeViewModel(def, value));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi tải thuộc tính động: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SaveContentEntryAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // 1. Lưu ContentEntry (để đảm bảo nó có ID)
                // Đặt BookId phòng trường hợp Entry mới
                CurrentEntry.BookId = Book.Id;
                await _dataAccess.SaveContentEntryAsync(CurrentEntry);

                // 2. Lưu tất cả các giá trị (AttributeValue)
                foreach (var attrVM in DynamicAttributes)
                {
                    // Đảm bảo ID mục nội dung là chính xác
                    attrVM.Value.ContentEntryId = CurrentEntry.Id;
                    await _dataAccess.SaveAttributeValueAsync(attrVM.Value);
                }
                // --- Cần lưu cả Tags (dù logic này nằm trong TagSelectionPage) ---
                Title = "Sửa Nội dung"; // Cập nhật tiêu đề
                OnPropertyChanged(nameof(CurrentEntry)); // Kích hoạt IsVisible
                await Shell.Current.DisplayAlert("Thành công", "Đã lưu nội dung", "OK");
                // Không tự động quay lại, để người dùng có thể thêm/sửa Tag
                // await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi lưu nội dung: {ex.Message}");
                await Shell.Current.DisplayAlert("Lỗi", "Không thể lưu nội dung", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task OpenTagSelectionModalAsync()
        {
            if (CurrentEntry.Id == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng lưu nội dung trước khi thêm tag.", "OK");
                return;
            }

            // Điều hướng đến trang chọn Tag với bối cảnh là "Entry"
            await Shell.Current.GoToAsync("TagSelectionPage", true, new Dictionary<string, object>
            {
                { "Target", "Entry" },
                { "TargetId", CurrentEntry.Id }
            });
        }
    }
}