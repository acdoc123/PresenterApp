// File: ViewModels/BookSummaryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using System.Collections.ObjectModel;

namespace PresenterApp.ViewModels
{
    public partial class BookSummaryViewModel : ObservableObject
    {
        public Book Book { get; }

        [ObservableProperty]
        bool isExpanded = false;

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

        [RelayCommand]
        void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
        }
    }
}