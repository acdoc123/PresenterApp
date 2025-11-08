// File: Services/FileHelper.cs
using System;
using System.IO;
using System.Threading.Tasks;

namespace PresenterApp.Services
{
    public static class FileHelper
    {
        /// <summary>
        /// Sao chép một FileResult (từ FilePicker hoặc MediaPicker) vào 
        /// thư mục dữ liệu ứng dụng và trả về đường dẫn mới.
        /// </summary>
        public static async Task<string> CopyFileToAppData(FileResult fileResult)
        {
            // Tạo thư mục nếu chưa có
            var dataDir = Path.Combine(FileSystem.AppDataDirectory, "UserDataFiles");
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            // Tạo đường dẫn mới, duy nhất
            var newFileName = $"{Guid.NewGuid()}_{fileResult.FileName}";
            var newPath = Path.Combine(dataDir, newFileName);

            // Sao chép stream
            using (var stream = await fileResult.OpenReadAsync())
            using (var newStream = File.Create(newPath))
            {
                await stream.CopyToAsync(newStream);
            }
            return newPath;
        }

        /// <summary>
        /// Mở MediaPicker để chọn một ảnh.
        /// </summary>
        public static async Task<FileResult?> PickImageAsync()
        {
            try
            {
                return await MediaPicker.PickPhotoAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
                return null;
            }
        }

        /// <summary>
        /// Mở FilePicker để chọn một tệp PDF.
        /// </summary>
        public static async Task<FileResult?> PickPdfAsync()
        {
            try
            {
                var fileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "com.adobe.pdf" } },
                    { DevicePlatform.Android, new[] { "application/pdf" } },
                    { DevicePlatform.WinUI, new[] { ".pdf" } },
                    { DevicePlatform.MacCatalyst, new[] { "pdf" } },
                });
                return await FilePicker.PickAsync(new PickOptions { FileTypes = fileTypes });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể chọn PDF: {ex.Message}", "OK");
                return null;
            }
        }
    }
}