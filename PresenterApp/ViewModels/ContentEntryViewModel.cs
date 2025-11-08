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

            // KIỂM TRA LOẠI NAMEDTEXTLIST
            if (attribute.Type == FieldType.NamedTextList)
            {
                try
                {
                    // Giải mã JSON
                    var items = JsonSerializer.Deserialize<List<NamedTextEntry>>(value.Value);
                    var firstEntry = items?.FirstOrDefault();

                    if (firstEntry != null)
                    {
                        // Lấy nội dung và cắt ngắn nếu quá 50 ký tự
                        string contentSummary = firstEntry.Content ?? "";
                        if (contentSummary.Length > 50)
                        {
                            contentSummary = contentSummary.Substring(0, 50) + "...";
                        }

                        // Định dạng đẹp: "1. Lời 1..."
                        // Nếu tên (Name) trống, chỉ hiển thị nội dung
                        if (string.IsNullOrWhiteSpace(firstEntry.Name))
                        {
                            return contentSummary;
                        }
                        return $"{firstEntry.Name} {contentSummary}";
                    }
                    else
                    {
                        return $"({attribute.Name} trống)";
                    }
                }
                catch
                {
                    return "(Lỗi hiển thị danh sách)"; // Fallback nếu JSON bị lỗi
                }
            }

            // Logic cũ cho các loại khác (Text, Number, v.v.)
            return $"{value.Value}";
        }
    }
}