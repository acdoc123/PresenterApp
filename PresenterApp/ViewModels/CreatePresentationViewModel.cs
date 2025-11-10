using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using PresenterApp.Models;
using PresenterApp.Views;

namespace PresenterApp.ViewModels
{
    [QueryProperty(nameof(SelectedTheme), "SelectedThemeId")]
    [QueryProperty(nameof(SelectedStructure), "SelectedStructureId")]
    public partial class CreatePresentationViewModel : BaseViewModel, IQueryAttributable
    {
        // --- Các thuộc tính cho Picker ---
        [ObservableProperty]
        ObservableCollection<PresentationTheme> themes = new();

        [ObservableProperty]
        ObservableCollection<PresentationStructure> structures = new();

        [ObservableProperty]
        PresentationTheme? selectedTheme;

        [ObservableProperty]
        PresentationStructure? selectedStructure;

        // --- Các thuộc tính cho logic tạo slide ---
        [ObservableProperty]
        bool isStructureSelected;

        [ObservableProperty]
        ObservableCollection<PresentationComponentViewModel> structureComponents = new();

        [ObservableProperty]
        ObservableCollection<FlexibleContentBlock> freeSlides = new();

        private PresentationComponentViewModel? _currentTargetComponent;
        private readonly DataAccessService _dataAccess;

        public CreatePresentationViewModel(DataAccessService dataAccess) : base()
        {
            _dataAccess = dataAccess;
            Title = "Tạo Trình Chiếu Mới";
            // Tải dữ liệu cho Picker khi ViewModel được tạo
            _ = LoadPickerDataAsync();
        }

        async Task LoadPickerDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var themeList = await _dataAccess.GetPresentationThemesAsync();
                var structureList = await _dataAccess.GetPresentationStructuresAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Themes.Clear();
                    Structures.Clear();
                    foreach (var t in themeList.OrderBy(t => t.Name)) Themes.Add(t);
                    foreach (var s in structureList.OrderBy(s => s.Name)) Structures.Add(s);
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải Themes/Structures: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedStructureChanged(PresentationStructure? value)
        {
            IsStructureSelected = value != null;
            _ = LoadComponentsForStructureAsync(value);
        }

        async Task LoadComponentsForStructureAsync(PresentationStructure? structure)
        {
            StructureComponents.Clear();
            if (structure == null) return;

            try
            {
                // Giả sử bạn có phương thức này trong DataAccessService
                var components = await _dataAccess.GetComponentsForStructureAsync(structure.Id);

                foreach (var comp in components.OrderBy(c => c.Order))
                {
                    var componentVM = new PresentationComponentViewModel(comp, _dataAccess);
                    // await componentVM.LoadContentBlocksAsync(); // (Tùy chọn: Tải nội dung đã có)
                    StructureComponents.Add(componentVM);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể tải các phần của cấu trúc: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task AddContentToComponent(PresentationComponentViewModel componentVM)
        {
            if (componentVM == null) return;

            _currentTargetComponent = componentVM;
            var component = componentVM.Component;

            var navParams = new ShellNavigationQueryParameters
            {
                { "IsSelectionMode", "True" },
                { "BookTypeId", component.BookTypeId?.ToString() ?? "0" },
                { "BookId", component.BookId?.ToString() ?? "0" },
                { "TagId", component.TagId?.ToString() ?? "0" }
            };

            await Shell.Current.GoToAsync(nameof(ContentSearchPage), navParams);
        }

        [RelayCommand]
        async Task AddFreeSlide()
        {
            _currentTargetComponent = null;

            var navParams = new ShellNavigationQueryParameters
            {
                { "IsSelectionMode", "True" },
                { "BookTypeId", "0" },
                { "BookId", "0" },
                { "TagId", "0" }
            };

            await Shell.Current.GoToAsync(nameof(ContentSearchPage), navParams);
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Nhận ID nội dung trả về từ ContentSearchPage
            if (query.TryGetValue("SelectedContentEntryId", out var contentId) &&
                int.TryParse(contentId.ToString(), out int selectedId))
            {
                var contentEntry = await _dataAccess.GetAsync<ContentEntry>(selectedId);
                if (contentEntry == null) return;

                // TƯƠNG LAI: Mở cửa sổ tùy chỉnh slide
                // ...

                // TẠM THỜI: Thêm trực tiếp
                var newBlock = new FlexibleContentBlock
                {
                    ContentEntryId = contentEntry.Id,
                    ContentEntry = contentEntry,
                    Order = (_currentTargetComponent?.ContentBlocks.Count ?? FreeSlides.Count) + 1,
                };

                if (_currentTargetComponent != null)
                {
                    _currentTargetComponent.ContentBlocks.Add(newBlock);
                    // (Cần thêm logic lưu newBlock vào DB)
                }
                else
                {
                    FreeSlides.Add(newBlock);
                    // (Cần thêm logic lưu newBlock vào DB)
                }

                _currentTargetComponent = null;
            }

            // Xử lý các query property đã có (SelectedThemeId, SelectedStructureId)
            if (query.TryGetValue("SelectedThemeId", out var themeId) && int.TryParse(themeId.ToString(), out int tId))
            {
                if (Themes.Count == 0) await LoadPickerDataAsync();
                SelectedTheme = Themes.FirstOrDefault(t => t.Id == tId);
            }
            if (query.TryGetValue("SelectedStructureId", out var structureId) && int.TryParse(structureId.ToString(), out int sId))
            {
                if (Structures.Count == 0) await LoadPickerDataAsync();
                SelectedStructure = Structures.FirstOrDefault(s => s.Id == sId);
            }
        }
    }
}