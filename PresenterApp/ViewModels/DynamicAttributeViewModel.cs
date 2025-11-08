// File: ViewModels/DynamicAttributeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Collections.Specialized;
using System.ComponentModel;
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
        public string StringValue
        {
            get => stringValue;
            set
            {
                if (SetProperty(ref stringValue, value))
                {
                    if (Definition.Type != FieldType.NamedTextList)
                    {
                        Value.Value = value;
                    }

                    // THÊM: Gọi OnPropertyChanged thủ công
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(IsFileSelected));
                }
            }
        }
        // 4. Danh sách cho các mục con (chỉ dùng cho FieldType.NamedTextList)
        [ObservableProperty]
        ObservableCollection<NamedTextEntry> namedEntryList;

        // Thuộc tính để hiển thị tên file (cho PDF)
        public string FileName => Path.GetFileName(StringValue);

        // Thuộc tính để biết file đã được chọn hay chưa
        public bool IsFileSelected => !string.IsNullOrWhiteSpace(StringValue);

        public DynamicAttributeViewModel(AttributeDefinition definition, AttributeValue value)
        {
            Definition = definition;
            Value = value;

            if (Definition.Type == FieldType.NamedTextList)
            {
                // Khởi tạo danh sách
                NamedEntryList = new ObservableCollection<NamedTextEntry>();

                // Deserialize dữ liệu từ JSON (nếu có)
                if (!string.IsNullOrWhiteSpace(value.Value))
                {
                    try
                    {
                        var items = JsonSerializer.Deserialize<List<NamedTextEntry>>(value.Value);
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                // Gắn sự kiện để theo dõi thay đổi Tên/Nội dung
                                item.PropertyChanged += OnEntryPropertyChanged;
                                NamedEntryList.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi deserialize NamedTextList: {ex.Message}");
                    }
                }

                // Gắn sự kiện để theo dõi Thêm/Xóa mục
                NamedEntryList.CollectionChanged += OnListCollectionChanged;
            }
            else
            {
                // Logic cho các loại (Text, Image, v.v.)
                stringValue = value.Value;
            }
        }
        // Được gọi khi Tên/Nội dung của một mục con thay đổi
        private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SerializeListToValue();
        }

        // Được gọi khi một mục con được Thêm/Xóa khỏi danh sách
        private void OnListCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Nếu thêm item mới, gắn sự kiện
            if (e.NewItems != null)
            {
                foreach (NamedTextEntry item in e.NewItems)
                {
                    item.PropertyChanged += OnEntryPropertyChanged;
                }
            }
            // Nếu xóa item, gỡ sự kiện
            if (e.OldItems != null)
            {
                foreach (NamedTextEntry item in e.OldItems)
                {
                    item.PropertyChanged -= OnEntryPropertyChanged;
                }
            }
            SerializeListToValue();
        }

        // Hàm chính: chuyển đổi danh sách thành chuỗi JSON và lưu vào Model
        private void SerializeListToValue()
        {
            Value.Value = JsonSerializer.Serialize(NamedEntryList);
        }

        [RelayCommand]
        void AddNewNamedEntry()
        {
            // Thêm mục mới (sự kiện CollectionChanged sẽ tự động serialize)
            NamedEntryList.Add(new NamedTextEntry { Name = "Mục mới", Content = "" });
        }

        [RelayCommand]
        async Task DeleteNamedEntry(NamedTextEntry entry)
        {
            if (entry == null) return;

            string entryName = string.IsNullOrWhiteSpace(entry.Name) ? "(mục không tên)" : entry.Name;

            bool confirm = await Shell.Current.DisplayAlert(
                "Xác nhận Xóa",
                $"Bạn có chắc muốn xóa mục '{entryName}'?",
                "Có",
                "Không");

            if (confirm)
            {
                NamedEntryList.Remove(entry);
            }
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