// File: ViewModels/EditStructureViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace PresenterApp.ViewModels
{
    public partial class EditStructureViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        PresentationStructure currentStructure;

        [ObservableProperty]
        ObservableCollection<PresentationComponentViewModel> components = new();

        [ObservableProperty]
        string newComponentName;

        public EditStructureViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async void Initialize(PresentationStructure structure)
        {
            CurrentStructure = structure;
            Title = structure.Id == 0 ? "Tạo Cấu trúc Trình chiếu" : $"Sửa: {structure.Name}";

            if (CurrentStructure.Id != 0)
            {
                await LoadComponentsAsync();
            }
        }

        [RelayCommand]
        async Task LoadComponentsAsync()
        {
            Components.Clear();
            // *** SỬA LỖI 2: Bọc (wrap) Model trong ViewModel ***
            var items = await _dataAccess.GetComponentsForStructureAsync(CurrentStructure.Id);
            foreach (var item in items)
            {
                Components.Add(new PresentationComponentViewModel(item));
            }
        }

        [RelayCommand]
        async Task AddComponentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewComponentName)) return;

            if (CurrentStructure.Id == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng lưu Cấu trúc trước khi thêm thành phần.", "OK");
                return;
            }

            var newComponent = new PresentationComponent
            {
                Name = NewComponentName,
                StructureId = CurrentStructure.Id,
                DisplayOrder = Components.Count
            };

            await _dataAccess.SaveComponentAsync(newComponent);
            Components.Add(new PresentationComponentViewModel(newComponent));
            NewComponentName = string.Empty;
        }

        [RelayCommand]
        async Task DeleteComponentAsync(PresentationComponentViewModel componentVM)
        {
            if (componentVM == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Xóa thành phần '{componentVM.Name}'?", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeleteComponentAsync(componentVM.Component);
                Components.Remove(componentVM);
                await SaveAsync();
            }
        }

        [RelayCommand]
        async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentStructure.Name))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập Tên Cấu trúc", "OK");
                return;
            }

            // 1. Lưu Cấu trúc
            await _dataAccess.SavePresentationStructureAsync(CurrentStructure);
            Title = $"Sửa: {CurrentStructure.Name}";

            // 2. Cập nhật tên từ Entry (nếu người dùng sửa)
            foreach (var vm in Components)
            {
                vm.SaveChanges();
            }

            // 3. Lưu lại tất cả Components (để cập nhật thứ tự)
            var componentModels = Components.Select(vm => vm.Component).ToList();
            await _dataAccess.SaveComponentsForStructureAsync(CurrentStructure.Id, componentModels);

            await Shell.Current.DisplayAlert("Thành công", "Đã lưu Cấu trúc", "OK");

            await Shell.Current.GoToAsync("..");
        }


        [RelayCommand]
        async Task DeleteAsync()
        {
            if (CurrentStructure.Id == 0) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa '{CurrentStructure.Name}'?", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeletePresentationStructureAsync(CurrentStructure);
                await Shell.Current.GoToAsync("..");
            }
        }


        [RelayCommand]
        void ItemDragged(PresentationComponentViewModel componentVM)
        {
            if (componentVM != null)
                componentVM.IsDragging = true;
        }

        [RelayCommand]
        void ItemDraggedOver(PresentationComponentViewModel componentVM)
        {
            // Có thể thêm logic highlight (nếu cần)
        }

        [RelayCommand]
        void ItemDragLeave(PresentationComponentViewModel componentVM)
        {
            // Có thể thêm logic highlight (nếu cần)
        }

        [RelayCommand]
        void ItemDropped(PresentationComponentViewModel componentVM)
        {
            var itemToMove = Components.FirstOrDefault(x => x.IsDragging);
            if (itemToMove == null || componentVM == null)
                return;

            int insertAtIndex = Components.IndexOf(componentVM);
            if (insertAtIndex < 0 || insertAtIndex >= Components.Count)
                return;

            Components.Remove(itemToMove);
            Components.Insert(insertAtIndex, itemToMove);

            // Cập nhật lại DisplayOrder
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Component.DisplayOrder = i; // Cập nhật Model
                Components[i].IsDragging = false; // Reset trạng thái UI
            }
        }
    }
}