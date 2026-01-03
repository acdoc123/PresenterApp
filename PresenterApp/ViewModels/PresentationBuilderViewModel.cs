// File: ViewModels/PresentationBuilderViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using PresenterApp.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    [QueryProperty(nameof(Structure), "Structure")]
    [QueryProperty(nameof(Theme), "Theme")]
    public partial class PresentationBuilderViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        PresentationStructure structure;

        [ObservableProperty]
        PresentationTheme theme;

        [ObservableProperty]
        ObservableCollection<PresentationSlideItem> items = new();

        [ObservableProperty]
        PresentationSlideItem selectedItem;

        public PresentationBuilderViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            Title = "Biên tập Trình chiếu";
        }

        async partial void OnStructureChanged(PresentationStructure value)
        {
            if (value == null) return;
            await InitializeFromStructure(value);
        }

        private async Task InitializeFromStructure(PresentationStructure structure)
        {
            Items.Clear();
            if (structure.Id == 0) return; // Không có cấu trúc, bắt đầu trống

            var components = await _dataAccess.GetComponentsForStructureAsync(structure.Id);
            foreach (var comp in components)
            {
                // Tạo mục slide từ component
                var item = new PresentationSlideItem
                {
                    SourceComponent = comp,
                    Content = $"[{comp.Name}] - Chưa có nội dung", // Placeholder
                    // TODO: Lấy style mặc định từ Theme nếu có
                };
                Items.Add(item);
            }

            if (Items.Any())
            {
                SelectedItem = Items.First();
            }
        }

        [RelayCommand]
        void AddNewItem()
        {
            var newItem = new PresentationSlideItem
            {
                Content = "Nội dung mới...",
                FontSize = 32
            };
            Items.Add(newItem);
            SelectedItem = newItem;
        }

        [RelayCommand]
        void RemoveItem(PresentationSlideItem item)
        {
            if (Items.Contains(item))
            {
                Items.Remove(item);
            }
        }

        [RelayCommand]
        async Task PickContent()
        {
            if (SelectedItem == null) return;

            // Mở trang tìm kiếm nội dung (cần implement callback hoặc messaging để lấy lại kết quả)
            // Tạm thời dùng Shell Navigation với tham số trả về
            // Ở đây tôi giả lập việc chọn nội dung

            // NOTE: Bạn cần implement trang SearchContentPage trả về ContentEntry
            // Ví dụ: await Shell.Current.GoToAsync(nameof(ContentSearchPage), ...);

            // Hiện tại tôi sẽ hiển thị thông báo giả lập
             await Shell.Current.DisplayAlert("Thông báo", "Tính năng chọn nội dung từ DB sẽ được cập nhật sau.", "OK");
        }

        [RelayCommand]
        async Task GeneratePptx()
        {
            if (Items.Count == 0)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Danh sách trống!", "OK");
                return;
            }

            // Gọi Service tạo PPTX ở đây
            await Shell.Current.DisplayAlert("Demo", $"Đang tạo file PPTX với {Items.Count} mục...", "OK");
        }
    }
}