// File: ViewModels/EditThemeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Drawing;
using System.Linq;

namespace PresenterApp.ViewModels
{
    public partial class EditThemeViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        PresentationTheme currentTheme;

        [ObservableProperty]
        ObservableCollection<string> slideSizes;

        [ObservableProperty]
        string selectedSlideSize;

        [ObservableProperty]
        ObservableCollection<SlideOrientation> orientations;

        [ObservableProperty]
        SlideOrientation selectedOrientation;

        [ObservableProperty]
        bool isCustomSize;

        [ObservableProperty]
        double customWidth;

        [ObservableProperty]
        double customHeight;

        [ObservableProperty]
        string backgroundColorHex;

        [ObservableProperty]
        string textColorHex;

        public ObservableCollection<string> FontList { get; } = new ObservableCollection<string>
        {
            "OpenSansRegular",
            "OpenSansSemibold",
            "Arial",
            "Calibri",
            "Times New Roman"
        };

        [ObservableProperty]
        string selectedTitleFont;

        [ObservableProperty]
        string selectedContentFont;

        public EditThemeViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            SlideSizes = new ObservableCollection<string> { "16:9", "4:3", "Custom" };
            Orientations = new ObservableCollection<SlideOrientation>(System.Enum.GetValues(typeof(SlideOrientation)).Cast<SlideOrientation>());
        }

        public void Initialize(PresentationTheme theme)
        {
            CurrentTheme = theme;
            Title = theme.Id == 0 ? "Tạo Giao diện chủ đề" : $"Sửa: {theme.Name}";

            if (theme.SlideWidth == 13.333 && theme.SlideHeight == 7.5) // 16:9
                SelectedSlideSize = "16:9";
            else if (theme.SlideWidth == 10 && theme.SlideHeight == 7.5) // 4:3
                SelectedSlideSize = "4:3";
            else
            {
                SelectedSlideSize = "Custom";
                CustomWidth = theme.SlideWidth;
                CustomHeight = theme.SlideHeight;
            }

            SelectedOrientation = theme.Orientation;

            BackgroundColorHex = theme.BackgroundColor;
            TextColorHex = theme.TextColor;

            SelectedTitleFont = FontList.Contains(theme.TitleFont) ? theme.TitleFont : FontList[0];
            SelectedContentFont = FontList.Contains(theme.ContentFont) ? theme.ContentFont : FontList[0];
        }

        partial void OnSelectedSlideSizeChanged(string value)
        {
            IsCustomSize = (value == "Custom");
            if (value == "16:9")
            {
                CustomWidth = 13.333;
                CustomHeight = 7.5;
            }
            else if (value == "4:3")
            {
                CustomWidth = 10;
                CustomHeight = 7.5;
            }
            OnPropertyChanged(nameof(IsCustomSize));
        }

        partial void OnSelectedOrientationChanged(SlideOrientation value)
        {
            if (!IsCustomSize)
            {
                var w = CustomWidth;
                var h = CustomHeight;
                if ((value == SlideOrientation.Landscape && w < h) ||
                    (value == SlideOrientation.Portrait && h < w))
                {
                    CustomWidth = h;
                    CustomHeight = w;
                }
            }
        }


        [RelayCommand]
        async Task LoadFromPptxAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".pptx" } },
                        { DevicePlatform.MacCatalyst, new[] { "pptx" } },
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation" } },
                        { DevicePlatform.iOS, new[] { "org.openxmlformats.presentationml.presentation" } }
                    })
                });
                if (file == null) return;

                using (var stream = await file.OpenReadAsync())
                {
                    using (PresentationDocument presDoc = PresentationDocument.Open(stream, false))
                    {
                        var presentationPart = presDoc.PresentationPart;
                        if (presentationPart == null) return;

                        var slideSize = presentationPart.Presentation.SlideSize;
                        if (slideSize?.Cx != null && slideSize?.Cy != null)
                        {
                            CustomWidth = slideSize.Cx / 914400.0;
                            CustomHeight = slideSize.Cy / 914400.0;
                            SelectedSlideSize = "Custom";
                        }

                        SelectedOrientation = CustomWidth >= CustomHeight ? SlideOrientation.Landscape : SlideOrientation.Portrait;

                        var slideMaster = presentationPart.SlideMasterParts.FirstOrDefault()?.SlideMaster;
                        if (slideMaster == null)
                        {
                            await Shell.Current.DisplayAlert("Lỗi", "Không tìm thấy Slide Master trong file.", "OK");
                            return;
                        }

                        var titleStyle = slideMaster.TextStyles?.TitleStyle;
                        var titleLvl1Props = titleStyle?.GetFirstChild<Level1ParagraphProperties>();
                        var titleDefRunProps = titleLvl1Props?.GetFirstChild<DefaultRunProperties>();
                        var titleFont = titleDefRunProps?.GetFirstChild<LatinFont>()?.Typeface;
                        if (!string.IsNullOrEmpty(titleFont?.Value))
                            SelectedTitleFont = FontList.Contains(titleFont.Value) ? titleFont.Value : FontList[0];

                        var bodyStyle = slideMaster.TextStyles?.BodyStyle;
                        var bodyLvl1Props = bodyStyle?.GetFirstChild<Level1ParagraphProperties>();
                        var bodyDefRunProps = bodyLvl1Props?.GetFirstChild<DefaultRunProperties>();

                        var contentFont = bodyDefRunProps?.GetFirstChild<LatinFont>()?.Typeface;
                        if (!string.IsNullOrEmpty(contentFont?.Value))
                            SelectedContentFont = FontList.Contains(contentFont.Value) ? contentFont.Value : FontList[0];

                        var contentFill = bodyDefRunProps?.GetFirstChild<SolidFill>();
                        var contentColor = contentFill?.RgbColorModelHex?.Val;
                        if (contentColor != null)
                            TextColorHex = $"#{contentColor.Value}";

                        var bgProps = slideMaster.CommonSlideData?.Background?.BackgroundProperties;
                        var bgFill = bgProps?.GetFirstChild<SolidFill>();
                        var bgColor = bgFill?.RgbColorModelHex?.Val;
                        if (bgColor != null)
                        {
                            BackgroundColorHex = $"#{bgColor.Value}";
                        }

                        await Shell.Current.DisplayAlert("Thành công", "Đã tải thông tin từ file PPTX. Vui lòng đặt tên và lưu lại.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể đọc file: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentTheme.Name))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập Tên Giao diện", "OK");
                return;
            }

            CurrentTheme.SlideWidth = CustomWidth;
            CurrentTheme.SlideHeight = CustomHeight;
            CurrentTheme.Orientation = SelectedOrientation;

            CurrentTheme.BackgroundColor = BackgroundColorHex;
            CurrentTheme.TextColor = TextColorHex;

            CurrentTheme.TitleFont = SelectedTitleFont;
            CurrentTheme.ContentFont = SelectedContentFont;

            await _dataAccess.SavePresentationThemeAsync(CurrentTheme);
            await Shell.Current.DisplayAlert("Thành công", "Đã lưu Giao diện", "OK");
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task DeleteAsync()
        {
            if (CurrentTheme.Id == 0) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa '{CurrentTheme.Name}'?", "Có", "Không");
            if (confirm)
            {
                await _dataAccess.DeletePresentationThemeAsync(CurrentTheme);
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}