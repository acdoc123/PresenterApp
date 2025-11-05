// File: ViewModels/BookSummaryViewModel.cs
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class BookSummaryViewModel : ObservableObject
    {
        public Book Book { get; }
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        bool isExpanded;

        [ObservableProperty]
        bool isLoadingContent;

        [ObservableProperty]
        ObservableCollection<ContentEntryViewModel> contentEntries = new();

        public BookSummaryViewModel(Book book, DataAccessService dataAccess)
        {
            Book = book;
            _dataAccess = dataAccess;
        }

        [RelayCommand]
        async Task ToggleExpandAsync()
        {
            IsExpanded = !IsExpanded;
            // Chỉ tải lần đầu tiên
            if (IsExpanded && ContentEntries.Count == 0)
            {
                await LoadContentAsync();
            }
        }

        [RelayCommand]
        async Task LoadContentAsync()
        {
            if (IsLoadingContent) return;
            IsLoadingContent = true;

            try
            {
                // Lấy các thuộc tính của sách này
                var commonAttrs = await _dataAccess.GetAttributesForBookTypeAsync(Book.BookTypeId);
                var privateAttrs = await _dataAccess.GetAttributesForBookAsync(Book.Id);
                var allAttributes = commonAttrs.Concat(privateAttrs).ToList();

                var entries = await _dataAccess.GetContentEntriesAsync(Book.Id);

                var loadTasks = new List<Task>();
                foreach (var entry in entries)
                {
                    var vm = new ContentEntryViewModel(entry);
                    ContentEntries.Add(vm);
                    loadTasks.Add(vm.LoadSummaryAsync(_dataAccess, allAttributes));
                }
                await Task.WhenAll(loadTasks);
            }
            finally
            {
                IsLoadingContent = false;
            }
        }
    }
}