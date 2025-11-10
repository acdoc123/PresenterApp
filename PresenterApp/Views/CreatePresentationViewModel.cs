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
                var defaultTheme = new PresentationTheme { Id = 0, Name = "Tạo mới..." };
                PresentationThemes.Add(defaultTheme);
                foreach (var theme in themes) PresentationThemes.Add(theme);

                var lastThemeId = Preferences.Get("LastThemeId", 0);
                SelectedTheme = PresentationThemes.FirstOrDefault(t => t.Id == lastThemeId) ?? defaultTheme;

                PresentationStructures.Clear();
                var structures = await _dataAccess.GetPresentationStructuresAsync();
                var defaultStructure = new PresentationStructure { Id = 0, Name = "Tạo mới..." };
                PresentationStructures.Add(defaultStructure);
                foreach (var structure in structures) PresentationStructures.Add(structure);

                var lastStructureId = Preferences.Get("LastStructureId", 0);
                SelectedStructure = PresentationStructures.FirstOrDefault(s => s.Id == lastStructureId) ?? defaultStructure;
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
    }
}