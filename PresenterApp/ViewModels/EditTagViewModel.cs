// File: ViewModels/EditTagViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class EditTagViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        Tag currentTag;

        public EditTagViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public void Initialize(Tag tag)
        {
            CurrentTag = tag;
            Title = tag.Id == 0 ? "Thêm Tag Mới" : $"Sửa: {tag.Name}";
        }

        [RelayCommand]
        async Task SaveTagAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentTag.Name))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập Tên Tag", "OK");
                return;
            }

            await _dataAccess.SaveTagAsync(CurrentTag);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task DeleteTagAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa '{CurrentTag.Name}'?", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeleteTagAsync(CurrentTag);
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}