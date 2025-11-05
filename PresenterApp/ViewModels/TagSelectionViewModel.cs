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
        private int _bookId;

        [ObservableProperty]
        ObservableCollection<SelectableTag> allTags = new();

        [ObservableProperty]
        string newTagName;

        public TagSelectionViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public void Initialize(int bookId)
        {
            _bookId = bookId;
        }

        [RelayCommand]
        async Task LoadTagsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var allTagsFromDb = await _dataAccess.GetTagsAsync();
                var selectedTagIds = new HashSet<int>();
                if (_bookId != 0)
                {
                    var selectedBookTags = await _dataAccess.GetTagsForBookAsync(_bookId);
                    selectedTagIds = selectedBookTags.Select(t => t.TagId).ToHashSet();
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

            // Thêm vào danh sách và tự động chọn
            var selectableTag = new SelectableTag { Tag = newTag, IsSelected = true };
            AllTags.Add(selectableTag);

            NewTagName = string.Empty;
        }

        [RelayCommand]
        async Task DeleteTagAsync(SelectableTag tagToDelete)
        {
            if (tagToDelete == null) return;

            // THÊM MỚI: Yêu cầu xác nhận
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
                // Lưu các tag đã chọn vào DB
                var tagsToSave = AllTags.Where(t => t.IsSelected).Select(t => t.Tag).ToList();
                await _dataAccess.SetTagsForBookAsync(_bookId, tagsToSave);

                // Quay lại trang trước
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể lưu tags: {ex.Message}", "OK");
            }
        }
    }
}