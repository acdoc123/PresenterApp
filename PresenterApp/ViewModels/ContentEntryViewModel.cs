// File: Models/ContentEntryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using PresenterApp.Services;
using PresenterApp.Models;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    // Lớp ViewModel này bao bọc ContentEntry để hiển thị tóm tắt động
    public partial class ContentEntryViewModel : ObservableObject
    {
        public ContentEntry Entry { get; set; }

        [ObservableProperty]
        string summaryText = "Đang tải..."; // Nội dung tóm tắt

        public ContentEntryViewModel(ContentEntry entry)
        {
            Entry = entry;
        }

        // Tải nội dung của thuộc tính đầu tiên
        public async Task LoadSummaryAsync(DataAccessService dataAccess, AttributeDefinition? firstAttribute)
        {
            if (firstAttribute == null)
            {
                // Fallback nếu sách không có thuộc tính
                SummaryText = $"Nội dung thêm ngày {Entry.DateAdded:dd/MM/yyyy}";
                return;
            }

            // Tìm giá trị của thuộc tính đầu tiên
            var value = await dataAccess.GetAttributeValueAsync(Entry.Id, firstAttribute.Id);

            if (value != null && !string.IsNullOrWhiteSpace(value.Value))
            {
                SummaryText = $"{value.Value}";
            }
            else
            {
                // Fallback nếu thuộc tính đầu tiên không có giá trị
                SummaryText = $"Nội dung (không có {firstAttribute.Name})";
            }
        }
    }
}