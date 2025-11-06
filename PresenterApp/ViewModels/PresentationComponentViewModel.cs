// File: ViewModels/PresentationComponentViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using PresenterApp.Models;

namespace PresenterApp.ViewModels
{
    public partial class PresentationComponentViewModel : ObservableObject
    {
        public PresentationComponent Component { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Name))] // Cập nhật Name nếu ComponentName thay đổi
        string componentName; // Dùng cho Entry binding để sửa tên

        [ObservableProperty]
        bool isDragging; // Thuộc tính trạng thái UI

        // Các thuộc tính proxy (ủy quyền) tới Model
        public int Id => Component.Id;
        public int StructureId { get => Component.StructureId; set => Component.StructureId = value; }
        public int DisplayOrder { get => Component.DisplayOrder; set => Component.DisplayOrder = value; }

        // Dùng Name cho dễ, nhưng bind Entry vào ComponentName
        public string Name => Component.Name;

        public PresentationComponentViewModel(PresentationComponent component)
        {
            Component = component;
            componentName = component.Name;
        }

        // Hàm này sẽ được gọi trước khi lưu để cập nhật Model
        public void SaveChanges()
        {
            Component.Name = ComponentName;
        }
    }
}