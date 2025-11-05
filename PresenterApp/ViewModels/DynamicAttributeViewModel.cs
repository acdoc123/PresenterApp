// File: ViewModels/DynamicAttributeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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


        // 1. Đây là trường private lưu trữ giá trị
        private string stringValue;

        // 2. Đây là thuộc tính đầy đủ mà UI liên kết (bind) tới
        [NotifyPropertyChangedFor(nameof(FileName))]
        [NotifyPropertyChangedFor(nameof(IsFileSelected))]
        public string StringValue
        {
            get => stringValue;
            set
            {
                // 3. Sử dụng SetProperty để thông báo cho UI biết giá trị đã thay đổi
                if (SetProperty(ref stringValue, value))
                {
                    // Cập nhật giá trị của Model (Value.Value) ngay khi
                    // UI (Entry, Editor...) cập nhật thuộc tính này.
                    Value.Value = value;
                }
            }
        }

        // Thuộc tính để hiển thị tên file (cho PDF)
        public string FileName => Path.GetFileName(StringValue);

        // Thuộc tính để biết file đã được chọn hay chưa
        public bool IsFileSelected => !string.IsNullOrWhiteSpace(StringValue);

        public DynamicAttributeViewModel(AttributeDefinition definition, AttributeValue value)
        {
            Definition = definition;
            Value = value;
            // Khởi tạo giá trị ban đầu cho trường private
            stringValue = value.Value;
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
                // Cập nhật thuộc tính public (sẽ kích hoạt logic 'set' ở trên)
                StringValue = localPath;
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
                // Cập nhật thuộc tính public
                StringValue = localPath;
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
            // Cập nhật thuộc tính public
            StringValue = string.Empty;
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