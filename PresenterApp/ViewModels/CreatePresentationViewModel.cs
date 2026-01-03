// File: ViewModels/CreatePresentationViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PresenterApp.Models;
using PresenterApp.Services;
using PresenterApp.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PresenterApp.ViewModels
{
    public partial class CreatePresentationViewModel : BaseViewModel
    {
        private readonly DataAccessService _dataAccess;

        [ObservableProperty]
        ObservableCollection<PresentationTheme> presentationThemes = new();

        [ObservableProperty]
        PresentationTheme selectedTheme;

        [ObservableProperty]
        ObservableCollection<PresentationStructure> presentationStructures = new();

        [ObservableProperty]
        PresentationStructure selectedStructure;

        public CreatePresentationViewModel(DataAccessService dataAccess)
        {
            _dataAccess = dataAccess;
            Title = "Tạo Trình Chiếu";
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                PresentationThemes.Clear();
                var themes = await _dataAccess.GetPresentationThemesAsync();
                foreach (var theme in themes) PresentationThemes.Add(theme);

                var lastThemeId = Preferences.Get("LastThemeId", 0);
                SelectedTheme = PresentationThemes.FirstOrDefault(t => t.Id == lastThemeId) ?? PresentationThemes.FirstOrDefault();

                PresentationStructures.Clear();
                var structures = await _dataAccess.GetPresentationStructuresAsync();
                foreach (var structure in structures) PresentationStructures.Add(structure);

                var lastStructureId = Preferences.Get("LastStructureId", 0);
                SelectedStructure = PresentationStructures.FirstOrDefault(s => s.Id == lastStructureId) ?? PresentationStructures.FirstOrDefault();
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnSelectedThemeChanged(PresentationTheme value)
        {
            if (value != null)
            {
                Preferences.Set("LastThemeId", value.Id);
            }
        }

        partial void OnSelectedStructureChanged(PresentationStructure value)
        {
            if (value != null)
            {
                Preferences.Set("LastStructureId", value.Id);
            }
        }

        [RelayCommand]
        async Task GoToEditThemeAsync()
        {
            PresentationTheme themeToSend = (SelectedTheme == null || SelectedTheme.Id == 0)
                ? new PresentationTheme()
                : SelectedTheme;

            await Shell.Current.GoToAsync(nameof(EditThemePage), true, new Dictionary<string, object>
            {
                { "Item", themeToSend }
            });
        }

        [RelayCommand]
        async Task GoToEditStructureAsync()
        {
            PresentationStructure structureToSend = (SelectedStructure == null || SelectedStructure.Id == 0)
                ? new PresentationStructure()
                : SelectedStructure;

            await Shell.Current.GoToAsync(nameof(EditStructurePage), true, new Dictionary<string, object>
            {
                { "Item", structureToSend }
            });
        }

        [RelayCommand]
        async Task GoToPresentationBuilderAsync()
        {
            if (SelectedTheme == null)
            {
                await Shell.Current.DisplayAlert("Chưa chọn chủ đề", "Vui lòng chọn giao diện chủ đề trước", "OK");
                return;
            }

            var structureToSend = SelectedStructure ?? new PresentationStructure { Id = 0, Name = "" };

            await Shell.Current.GoToAsync(nameof(PresentationBuilderPage), true, new Dictionary<string, object>
            {
                { "Structure", structureToSend },
                { "Theme", SelectedTheme }
            });
        }
    }
}