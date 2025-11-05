// File: ViewModels/EditBookTypeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class EditBookTypeViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        BookType currentBookType;

        [ObservableProperty]
        ObservableCollection<AttributeDefinition> commonAttributes;

        [ObservableProperty]
        string newAttributeName;

        [ObservableProperty]
        FieldType newAttributeType;

        public ObservableCollection<FieldType> FieldTypes { get; } = new(System.Enum.GetValues(typeof(FieldType)).Cast<FieldType>());

        public EditBookTypeViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            CommonAttributes = new ObservableCollection<AttributeDefinition>();
        }

        public async void Initialize(BookType bookType)
        {
            CurrentBookType = bookType;
            Title = bookType.Id == 0 ? "Thêm Loại Sách Mới" : $"Sửa: {bookType.Name}";

            if (CurrentBookType.Id != 0)
            {
                await LoadAttributesAsync();
            }
        }

        [RelayCommand]
        async Task LoadAttributesAsync()
        {
            CommonAttributes.Clear();
            var attributes = await _dataAccess.GetAttributesForBookTypeAsync(CurrentBookType.Id);
            foreach (var attr in attributes)
            {
                CommonAttributes.Add(attr);
            }
        }

        [RelayCommand]
        async Task AddAttributeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewAttributeName) || CurrentBookType.Id == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng lưu Loại Sách trước khi thêm thuộc tính.", "OK");
                return;
            }

            var newAttr = new AttributeDefinition
            {
                Name = NewAttributeName,
                Type = NewAttributeType,
                BookTypeId = CurrentBookType.Id
            };

            await _dataAccess.SaveAttributeDefinitionAsync(newAttr);
            CommonAttributes.Add(newAttr);
            NewAttributeName = string.Empty;
        }

        [RelayCommand]
        async Task DeleteAttributeAsync(AttributeDefinition attribute)
        {
            if (attribute == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận Xóa", $"Bạn có chắc muốn xóa thuộc tính '{attribute.Name}'?", "Có", "Không");
            if (!confirm) return;
            await _dataAccess.DeleteAttributeDefinitionAsync(attribute);
            CommonAttributes.Remove(attribute);
        }

        [RelayCommand]
        async Task SaveBookTypeAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentBookType.Name))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập Tên Loại Sách", "OK");
                return;
            }

            await _dataAccess.SaveBookTypeAsync(CurrentBookType);
            Title = $"Sửa: {CurrentBookType.Name}"; // Cập nhật Title sau khi lưu lần đầu
            await Shell.Current.DisplayAlert("Thành công", "Đã lưu Loại Sách.", "OK");
        }

        [RelayCommand]
        async Task DeleteBookTypeAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa '{CurrentBookType.Name}'? Thao tác này KHÔNG THỂ hoàn tác.", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeleteBookTypeAsync(CurrentBookType);
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}