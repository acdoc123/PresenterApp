// File: ViewModels/ContentEntryViewModel.cs
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class ContentEntryViewModel : ObservableObject
    {
        public ContentEntry Entry { get; set; }

        [ObservableProperty]
        string summaryText1 = "Đang tải...";

        [ObservableProperty]
        string summaryText2 = "";

        private DataAccessService _dataAccess;
        private List<AttributeDefinition> _allAttributes;

        [ObservableProperty]
        bool isExpanded;

        [ObservableProperty]
        bool isLoadingDetails;

        [ObservableProperty]
        ObservableCollection<DynamicAttributeViewModel> detailedAttributes = new();

        public ContentEntryViewModel(ContentEntry entry)
        {
            Entry = entry;
        }

        public async Task LoadSummaryAsync(DataAccessService dataAccess, List<AttributeDefinition> allAttributes)
        {
            // Lưu lại để dùng cho LoadDetailsAsync
            _dataAccess = dataAccess;
            _allAttributes = allAttributes;

            var firstAttribute = allAttributes.ElementAtOrDefault(0);
            var secondAttribute = allAttributes.ElementAtOrDefault(1);

            // Tải thuộc tính đầu tiên
            if (firstAttribute != null)
            {
                var value1 = await dataAccess.GetAttributeValueAsync(Entry.Id, firstAttribute.Id);
                SummaryText1 = FormatSummary(firstAttribute, value1, false); // false = tóm tắt
            }
            else
            {
                SummaryText1 = $"Nội dung thêm ngày {Entry.DateAdded:dd/MM/yyyy}";
            }

            // Tải thuộc tính thứ hai
            if (secondAttribute != null)
            {
                var value2 = await dataAccess.GetAttributeValueAsync(Entry.Id, secondAttribute.Id);
                SummaryText2 = FormatSummary(secondAttribute, value2, false); // false = tóm tắt
            }
            else
            {
                SummaryText2 = "";
            }
        }
        private string FormatSummary(AttributeDefinition attribute, AttributeValue? value, bool isDetail)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.Value))
            {
                return $"{attribute.Name}: (trống)";
            }

            // Logic cho FlexibleContent
            if (attribute.Type == FieldType.FlexibleContent)
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<FlexibleContentBlock>>(value.Value);
                    var firstBlock = items?.FirstOrDefault(b => b.Type == ContentBlockType.NamedText);

                    if (firstBlock != null)
                    {
                        string name = firstBlock.Name;
                        string content = firstBlock.Content ?? "";

                        // Nếu là chi tiết, hiển thị nhiều hơn
                        int maxLength = isDetail ? 100 : 40;
                        if (content.Length > maxLength) content = content.Substring(0, maxLength) + "...";

                        if (string.IsNullOrWhiteSpace(name)) return content;
                        return $"{name} {content}";
                    }
                    else
                    {
                        var firstFile = items?.FirstOrDefault(b => b.Type != ContentBlockType.NamedText);
                        if (firstFile != null) return $"{attribute.Name}: ({firstFile.Type}: {firstFile.FileName})";
                        return $"{attribute.Name}: (trống)";
                    }
                }
                catch { return $"{attribute.Name}: (Lỗi định dạng nội dung)"; }
            }

            // Logic cho các loại khác
            string valueString = value.Value;
            if (!isDetail && (attribute.Type == FieldType.TextArea || attribute.Type == FieldType.Text))
            {
                if (valueString.Length > 50) valueString = valueString.Substring(0, 50) + "...";
            }

            return $"{attribute.Name} {valueString}";
        }

        [RelayCommand]
        async Task ToggleExpand()
        {
            IsExpanded = !IsExpanded;

            // Nếu đang mở rộng VÀ chi tiết chưa được tải
            if (IsExpanded && DetailedAttributes.Count == 0)
            {
                await LoadDetailsAsync();
            }
        }

        private async Task LoadDetailsAsync()
        {
            if (IsLoadingDetails || _dataAccess == null || _allAttributes == null) return;

            IsLoadingDetails = true;
            DetailedAttributes.Clear();

            try
            {
                // Tải tất cả các giá trị
                foreach (var attr in _allAttributes)
                {
                    var value = await _dataAccess.GetAttributeValueAsync(Entry.Id, attr.Id);

                    // Nếu giá trị là null, tạo một AttributeValue rỗng
                    if (value == null)
                    {
                        value = new AttributeValue
                        {
                            ContentEntryId = Entry.Id,
                            AttributeDefinitionId = attr.Id,
                            Value = string.Empty
                        };
                    }
                    var attrVM = new DynamicAttributeViewModel(attr, value);
                    DetailedAttributes.Add(attrVM);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi tải chi tiết: {ex.Message}");
            }
            finally
            {
                IsLoadingDetails = false;
            }
        }
    }
}