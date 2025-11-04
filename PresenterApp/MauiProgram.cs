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
            // Đăng ký Services
            builder.Services.AddSingleton<DataAccessService>();
            builder.Services.AddSingleton<PresentationGenerationService>();
            builder.Services.AddSingleton<PptxExportService>();

            // Đăng ký Views và ViewModels
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<HomeViewModel>();

            builder.Services.AddTransient<SongDetailPage>();
            builder.Services.AddTransient<SongDetailViewModel>();


            return builder.Build();
        }
    }
}