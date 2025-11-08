// File: ViewModels/DynamicAttributeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
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

        // 1. Cho các loại đơn giản (Text, TextArea, Number)
        private string stringValue;
        public string StringValue
        {
            get => stringValue;
            set
            {
                if (SetProperty(ref stringValue, value))
                {
                    // CHỈ cập nhật nếu là loại đơn giản
                    if (Definition.Type != FieldType.FlexibleContent)
                    {
                        Value.Value = value;
                    }
                }
            }
        }

        // 2. Cho loại FlexibleContent MỚI
        [ObservableProperty]
        ObservableCollection<FlexibleContentBlock> flexibleContentList;

        public DynamicAttributeViewModel(AttributeDefinition definition, AttributeValue value)
        {
            Definition = definition;
            Value = value;

            // --- CẬP NHẬT LOGIC KHỞI TẠO ---
            if (Definition.Type == FieldType.FlexibleContent)
            {
                FlexibleContentList = new ObservableCollection<FlexibleContentBlock>();
                if (!string.IsNullOrWhiteSpace(value.Value))
                {
                    try
                    {
                        // Deserialize danh sách các khối
                        var items = JsonSerializer.Deserialize<List<FlexibleContentBlock>>(value.Value);
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                item.PropertyChanged += OnBlockPropertyChanged; // Theo dõi thay đổi
                                FlexibleContentList.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi deserialize FlexibleContent: {ex.Message}");
                    }
                }
                // Gắn sự kiện để theo dõi Thêm/Xóa khối
                FlexibleContentList.CollectionChanged += OnBlockListCollectionChanged;
            }
            else
            {
                // Logic cũ cho Text, TextArea, Number
                stringValue = value.Value;
            }
            // -----------------------------
        }

        // --- HÀM XỬ LÝ SỰ KIỆN CHO FlexibleContent ---

        // Được gọi khi nội dung của một khối thay đổi (ví dụ: gõ chữ)
        private void OnBlockPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SerializeBlockListToValue();
        }

        // Được gọi khi một khối được Thêm/Xóa
        private void OnBlockListCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FlexibleContentBlock item in e.NewItems)
                {
                    item.PropertyChanged += OnBlockPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (FlexibleContentBlock item in e.OldItems)
                {
                    item.PropertyChanged -= OnBlockPropertyChanged;
                }
            }
            SerializeBlockListToValue();
        }

        // Chuyển đổi danh sách thành JSON và lưu vào Model
        private void SerializeBlockListToValue()
        {
            Value.Value = JsonSerializer.Serialize(FlexibleContentList);
        }

        // --- COMMANDS MỚI CHO FlexibleContent ---

        [RelayCommand]
        void AddNamedTextBlock()
        {
            // Thêm khối văn bản CÓ TÊN theo yêu cầu của bạn
            FlexibleContentList.Add(new FlexibleContentBlock { Type = ContentBlockType.NamedText, Name = "Tiêu đề" });
        }

        [RelayCommand]
        async Task AddImageBlock()
        {
            var file = await FileHelper.PickImageAsync();
            if (file == null) return;
            var newPath = await FileHelper.CopyFileToAppData(file);
            FlexibleContentList.Add(new FlexibleContentBlock { Type = ContentBlockType.Image, FilePath = newPath });
        }

        [RelayCommand]
        async Task AddPdfBlock()
        {
            var file = await FileHelper.PickPdfAsync();
            if (file == null) return;
            var newPath = await FileHelper.CopyFileToAppData(file);
            FlexibleContentList.Add(new FlexibleContentBlock { Type = ContentBlockType.Pdf, FilePath = newPath });
        }

        [RelayCommand]
        async Task DeleteBlock(FlexibleContentBlock block)
        {
            if (block == null) return;

            bool confirm = await Shell.Current.DisplayAlert("Xác nhận", "Xóa khối nội dung này?", "Có", "Không");
            if (confirm)
            {
                FlexibleContentList.Remove(block);
                // (Sau này có thể thêm logic xóa tệp trong AppData tại đây)
            }
        }

        [RelayCommand]
        async Task OpenFile(FlexibleContentBlock block)
        {
            if (block == null || !block.IsPdf || string.IsNullOrWhiteSpace(block.FilePath)) return;
            try
            {
                await Launcher.OpenAsync(new OpenFileRequest(block.FileName, new ReadOnlyFile(block.FilePath)));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Lỗi", $"Không thể mở file: {ex.Message}", "OK");
            }
        }
    }
}