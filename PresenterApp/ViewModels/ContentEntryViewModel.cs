// File: ViewModels/ContentEntryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using PresenterApp.Models;
using PresenterApp.Services;
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
                if (value1 != null && !string.IsNullOrWhiteSpace(value1.Value))
                    SummaryText1 = $"{firstAttribute.Name}: {value1.Value}";
                else
                    SummaryText1 = $"({firstAttribute.Name} trống)";
            }
            else
            {
                // Fallback nếu không có thuộc tính nào
                SummaryText1 = $"Nội dung thêm ngày {Entry.DateAdded:dd/MM/yyyy}";
            }

            // Tải thuộc tính thứ hai
            if (secondAttribute != null)
            {
                var value2 = await dataAccess.GetAttributeValueAsync(Entry.Id, secondAttribute.Id);
                if (value2 != null && !string.IsNullOrWhiteSpace(value2.Value))
                    SummaryText2 = $"{secondAttribute.Name}: {value2.Value}";
                else
                    SummaryText2 = $"({secondAttribute.Name} trống)";
            }
            else
            {
                SummaryText2 = ""; // Không hiển thị gì nếu không có thuộc tính thứ 2
            }
        }
    }
}