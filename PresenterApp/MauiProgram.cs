// File: MauiProgram.cs
using Microsoft.Extensions.Logging;
using PresenterApp.Services;
using PresenterApp.ViewModels;
using PresenterApp.Views;
using CommunityToolkit.Maui;

namespace PresenterApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
               .UseMauiApp<App>()
               .UseMauiCommunityToolkit()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                   fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
               });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Đăng ký Service
            builder.Services.AddSingleton<DataAccessService>();
            builder.Services.AddSingleton<FilterStateService>();

            // --- Đăng ký các Trang và ViewModel cho TabBar ---
            builder.Services.AddSingleton<ManagementDashboardPage>();
            builder.Services.AddSingleton<ManagementDashboardViewModel>();

            builder.Services.AddSingleton<CreatePresentationPage>();
            builder.Services.AddSingleton<CreatePresentationViewModel>();

            builder.Services.AddTransient<ContentSearchSharedViewModel>();
            builder.Services.AddSingleton<ContentSearchPage>();

            builder.Services.AddSingleton<SettingsPage>();
            builder.Services.AddSingleton<SettingsViewModel>();


            builder.Services.AddTransient<EditBookTypePage>();
            builder.Services.AddTransient<EditBookTypeViewModel>();

            builder.Services.AddTransient<EditBookPage>();
            builder.Services.AddTransient<EditBookViewModel>();

            builder.Services.AddTransient<EditTagPage>();
            builder.Services.AddTransient<EditTagViewModel>();

            builder.Services.AddTransient<EditContentEntryPage>();
            builder.Services.AddTransient<EditContentEntryViewModel>();

            builder.Services.AddTransient<TagSelectionPage>();
            builder.Services.AddTransient<TagSelectionViewModel>();

            builder.Services.AddTransient<EditThemePage>();
            builder.Services.AddTransient<EditThemeViewModel>();

            builder.Services.AddTransient<EditStructurePage>();
            builder.Services.AddTransient<EditStructureViewModel>();

            builder.Services.AddTransient<Views.Controls.ContentSearchSharedView>();

            return builder.Build();
        }
    }
}