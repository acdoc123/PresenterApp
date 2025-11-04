// File: ViewModels/HomeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using PresenterApp.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccessService;

        [ObservableProperty]
        ObservableCollection<Song> songs;

        public HomeViewModel(DataAccessService dataAccessService)
        {
            Title = "Danh sách bài hát";
            _dataAccessService = dataAccessService;
            Songs = new ObservableCollection<Song>();
        }
        [RelayCommand]
        async Task LoadSongsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var songList = await _dataAccessService.GetSongsAsync();
                Songs.Clear();
                foreach (var song in songList)
                {
                    Songs.Add(song);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task GoToSongDetailAsync(Song song)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "Song", song }
            };
            await Shell.Current.GoToAsync(nameof(SongDetailPage), true, navigationParameter);
        }

        [RelayCommand]
        async Task AddNewSongAsync()
        {
            // Tạo một bài hát mới để tránh lỗi null reference
            var newSong = new Song();
            var navigationParameter = new Dictionary<string, object>
            {
                { "Song", newSong }
            };
            await Shell.Current.GoToAsync(nameof(SongDetailPage), true, navigationParameter);
        }
    }
}