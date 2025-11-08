// File: ViewModels/ContentEntryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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

        public ContentEntryViewModel(ContentEntry entry)
        {
            Entry = entry;
        }

        public async Task LoadSummaryAsync(DataAccessService dataAccess, List<AttributeDefinition> allAttributes)
        {
            var firstAttribute = allAttributes.ElementAtOrDefault(0);
            var secondAttribute = allAttributes.ElementAtOrDefault(1);

            // Tải thuộc tính đầu tiên
            if (firstAttribute != null)
            {
                var value1 = await dataAccess.GetAttributeValueAsync(Entry.Id, firstAttribute.Id);
                SummaryText1 = FormatSummary(firstAttribute, value1);
            }
            else
            {
                SummaryText1 = $"Nội dung thêm ngày {Entry.DateAdded:dd/MM/yyyy}";
            }

            // Tải thuộc tính thứ hai
            if (secondAttribute != null)
            {
                var value2 = await dataAccess.GetAttributeValueAsync(Entry.Id, secondAttribute.Id);
                SummaryText2 = FormatSummary(secondAttribute, value2);
            }
            else
            {
                SummaryText2 = "";
            }
        }

        /// <summary>
        /// Hàm trợ giúp mới để định dạng văn bản tóm tắt
        /// </summary>
        private string FormatSummary(AttributeDefinition attribute, AttributeValue? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.Value))
            {
                return $"({attribute.Name} trống)";
            }

            if (attribute.Type == FieldType.FlexibleContent)
            {
                try
                {
                    // Giải mã JSON
                    var items = JsonSerializer.Deserialize<List<FlexibleContentBlock>>(value.Value);

                    // Tìm khối văn bản có tên (NamedText) đầu tiên
                    var firstBlock = items?.FirstOrDefault(b => b.Type == ContentBlockType.NamedText);

                    if (firstBlock != null)
                    {
                        string name = firstBlock.Name;
                        string content = firstBlock.Content ?? "";
                        if (content.Length > 40) content = content.Substring(0, 40) + "...";

                        return $"{name} {content}";
                    }
                    else // Nếu không có khối text, hiển thị file đầu tiên
                    {
                        var firstFile = items?.FirstOrDefault(b => b.Type != ContentBlockType.NamedText);
                        if (firstFile != null)
                        {
                            return $"({firstFile.Type} {firstFile.FileName})";
                        }
                        return $"({attribute.Name} trống)";
                    }
                }
                catch { return "(Lỗi định dạng nội dung)"; }
            }
            // Logic cũ cho Text, Number, TextArea
            return $"{value.Value}";
        }
    }
}