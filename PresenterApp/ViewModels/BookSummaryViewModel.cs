// File: ViewModels/BookSummaryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using PresenterApp.Models;
using System.Collections.ObjectModel;

namespace PresenterApp.ViewModels
{
    // ĐÃ VIẾT LẠI: ViewModel này giờ là một nhóm kết quả tìm kiếm
    public partial class BookSummaryViewModel : ObservableObject
    {
        public Book Book { get; }

        [ObservableProperty]
        bool isExpanded = true; // Luôn mở rộng khi hiển thị kết quả tìm kiếm

        [ObservableProperty]
        string resultCountText;

        [ObservableProperty]
        ObservableCollection<ContentEntryViewModel> contentEntries = new();

        public BookSummaryViewModel(Book book)
        {
            Book = book;
            UpdateResultCount();
        }

        public void UpdateResultCount()
        {
            ResultCountText = $"{ContentEntries.Count} nội dung";
        }
    }
}