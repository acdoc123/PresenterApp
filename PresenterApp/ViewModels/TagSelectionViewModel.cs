// File: ViewModels/TagSelectionViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class TagSelectionViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;
        // --- Dịch vụ trạng thái bộ lọc ---
        private readonly FilterStateService _filterStateService;

        // --- Thuộc tính cho QueryProperty ---
        [ObservableProperty]
        string target; // "Book", "Entry", hoặc "Filter"

        [ObservableProperty]
        int targetId; // BookId, EntryId, hoặc 0

        [ObservableProperty]
        ObservableCollection<SelectableTag> allTags = new();

        [ObservableProperty]
        string newTagName;

        public TagSelectionViewModel(DataAccessService dataAccess, FilterStateService filterStateService) // Inject dịch vụ mới
        {
            _dataAccess = dataAccess;
            _filterStateService = filterStateService; // Gán dịch vụ
        }

        // CẬP NHẬT: Phương thức Initialize bị loại bỏ vì đã dùng QueryProperty

        [RelayCommand]
        async Task LoadTagsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var allTagsFromDb = await _dataAccess.GetTagsAsync();
                var selectedTagIds = new HashSet<int>();

                // --- Tải các tag đã chọn dựa trên bối cảnh ---
                if (Target == "Book")
                {
                    var selectedBookTags = await _dataAccess.GetTagsForBookAsync(TargetId);
                    selectedTagIds = selectedBookTags.Select(t => t.TagId).ToHashSet();
                }
                else if (Target == "Entry")
                {
                    var selectedEntryTags = await _dataAccess.GetTagsForContentEntryAsync(TargetId);
                    selectedTagIds = selectedEntryTags.Select(t => t.TagId).ToHashSet();
                }
                else if (Target == "Filter")
                {
                    // Lấy các tag đã chọn từ dịch vụ trạng thái
                    selectedTagIds = _filterStateService.SelectedFilterTags.Select(t => t.Id).ToHashSet();
                }

                AllTags.Clear();
                foreach (var tag in allTagsFromDb)
                {
                    AllTags.Add(new SelectableTag
                    {
                        Tag = tag,
                        IsSelected = selectedTagIds.Contains(tag.Id)
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task AddNewTagAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTagName)) return;

            var newTag = new Tag { Name = NewTagName };
            await _dataAccess.SaveTagAsync(newTag);

            var selectableTag = new SelectableTag { Tag = newTag, IsSelected = true };
            AllTags.Add(selectableTag);

            NewTagName = string.Empty;
        }

        [RelayCommand]
        async Task DeleteTagAsync(SelectableTag tagToDelete)
        {
            if (tagToDelete == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Xác nhận Xóa", $"Bạn có chắc muốn xóa vĩnh viễn tag '{tagToDelete.Tag.Name}'? Thao tác này sẽ xóa tag khỏi tất cả các sách.", "Có", "Không");
            if (!confirm) return;

            await _dataAccess.DeleteTagAsync(tagToDelete.Tag);
            AllTags.Remove(tagToDelete);
        }

        [RelayCommand]
        async Task CloseAndSaveAsync()
        {
            try
            {
                var tagsToSave = AllTags.Where(t => t.IsSelected).Select(t => t.Tag).ToList();

                // --- Lưu vào đúng nơi dựa trên bối cảnh ---
                if (Target == "Book")
                {
                    await _dataAccess.SetTagsForBookAsync(TargetId, tagsToSave);
                }
                else if (Target == "Entry")
                {
                    await _dataAccess.SetTagsForContentEntryAsync(TargetId, tagsToSave);
                }
                else if (Target == "Filter")
                {
                    // Lưu trạng thái bộ lọc vào dịch vụ
                    _filterStateService.SelectedFilterTags = tagsToSave;
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu tags: {ex.Message}", "OK");
            }
        }
    }
}