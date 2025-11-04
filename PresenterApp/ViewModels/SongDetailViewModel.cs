// File: ViewModels/SongDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{

    public partial class SongDetailViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccessService;
        private readonly PresentationGenerationService _generationService;
        private readonly PptxExportService _exportService;

        [ObservableProperty]
        Song song = new();

        public SongDetailViewModel(DataAccessService dataAccessService, PresentationGenerationService generationService, PptxExportService exportService)
        {
            _dataAccessService = dataAccessService;
            _generationService = generationService;
            _exportService = exportService;
        }

        public void OnAppearing()
        {
            Song ??= new Song();
            Title = Song.Id == 0 ? "Thêm bài hát mới" : "Chỉnh sửa bài hát";
        }

        [RelayCommand]
        async Task SaveSongAsync()
        {
            if (string.IsNullOrWhiteSpace(Song.Title) || string.IsNullOrWhiteSpace(Song.Lyrics))
            {
                await Shell.Current.DisplayAlert("Lỗi", "Vui lòng nhập Tiêu đề và Lời bài hát.", "OK");
                return;
            }
            await _dataAccessService.SaveSongAsync(Song);
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        async Task DeleteSongAsync()
        {
            if (Song.Id == 0) return;
            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", $"Bạn có chắc muốn xóa bài hát '{Song.Title}'?", "Có", "Không");
            if (confirm)
            {
                await _dataAccessService.DeleteSongAsync(Song);
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        async Task ExportToPptxAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // Tạo một template mẫu
                var template = new PresentationTemplate
                {
                    Name = "Default Template",
                    DefaultRule = new SlideRule { LinesPerSlide = 4, IsBold = false, FontSize = 28 },
                    Rules = new List<SlideRule>
                    {
                        new SlideRule { TargetType = "Chorus", LinesPerSlide = 2, IsBold = true, FontSize = 32 }
                    }
                };

                var slideDefinitions = _generationService.GenerateSlides(Song, template);

                // Chọn nơi lưu file
                // Lưu ý: Việc chọn file trên Windows cần triển khai riêng nếu muốn có dialog chuyên nghiệp
                // Ở đây chúng ta sẽ lưu vào một thư mục mặc định
                string targetFileName = $"{SanitizeFileName(Song.Title)}.pptx";
                string filePath = Path.Combine(FileSystem.CacheDirectory, targetFileName);

                await _exportService.ExportToPptx(slideDefinitions, filePath);

                await Shell.Current.DisplayAlert("Thành công", $"Đã xuất file thành công!\nĐường dẫn: {filePath}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Đã xảy ra lỗi khi xuất file: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }
}