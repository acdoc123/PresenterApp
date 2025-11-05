// File: ViewModels/DynamicAttributeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml;
using PresenterApp.Models;
using System.Diagnostics;
using System.IO;

namespace PresenterApp.ViewModels
{
    public partial class DynamicAttributeViewModel : ObservableObject
    {
        public AttributeDefinition Definition { get; set; }
        public AttributeValue Value { get; set; }

        public string DisplayName => Definition.Name;
        public FieldType FieldType => Definition.Type;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FileName))]     // Cập nhật FileName khi StringValue thay đổi
        [NotifyPropertyChangedFor(nameof(IsFileSelected))] // Cập nhật IsFileSelected khi StringValue thay đổi
        string stringValue;

        // Thuộc tính để hiển thị tên file (cho PDF)
        public string FileName => Path.GetFileName(StringValue);

        // Thuộc tính để biết file đã được chọn hay chưa
        public bool IsFileSelected => !string.IsNullOrWhiteSpace(StringValue);

        public DynamicAttributeViewModel(AttributeDefinition definition, AttributeValue value)
        {
            Definition = definition;
            Value = value;
            StringValue = value.Value; // Khởi tạo giá trị ban đầu
        }

        // Lệnh chọn ảnh
        [RelayCommand]
        async Task PickImageAsync()
        {
            try
            {
                var photo = await MediaPicker.PickPhotoAsync();
                if (photo == null) return;

                // Sao chép file vào thư mục dữ liệu ứng dụng
                var localPath = await CopyFileToAppData(photo);
                StringValue = localPath; // Cập nhật UI
                Value.Value = localPath; // Cập nhật Model
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể chọn ảnh: {ex.Message}", "OK");
            }
        }

        // Lệnh chọn PDF
        [RelayCommand]
        async Task PickPdfAsync()
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

                var file = await FilePicker.PickAsync(new PickOptions { FileTypes = fileTypes });
                if (file == null) return;

                // Sao chép file
                var localPath = await CopyFileToAppData(file);
                StringValue = localPath; // Cập nhật UI
                Value.Value = localPath; // Cập nhật Model
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể chọn PDF: {ex.Message}", "OK");
            }
        }

        // Lệnh mở file (dùng cho PDF)
        [RelayCommand]
        async Task OpenFileAsync()
        {
            if (!IsFileSelected) return;
            try
            {
                await Launcher.OpenAsync(new OpenFileRequest(DisplayName, new ReadOnlyFile(StringValue)));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở file: {ex.Message}", "OK");
            }
        }

        // Lệnh xóa file đã chọn
        [RelayCommand]
        void ClearFile()
        {
            // TODO: (Nâng cao) Bạn có thể muốn xóa file vật lý khỏi AppData tại đây
            // File.Delete(StringValue);

            StringValue = string.Empty;
            Value.Value = string.Empty;
        }

        // Hàm tiện ích sao chép file vào thư mục AppData
        private async Task<string> CopyFileToAppData(FileResult fileResult)
        {
            // Tạo thư mục nếu chưa có
            var dataDir = Path.Combine(FileSystem.AppDataDirectory, "UserDataFiles");
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            // Tạo đường dẫn mới, duy nhất (để tránh trùng lặp)
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
    }
}