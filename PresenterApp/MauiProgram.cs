// File: MauiProgram.cs
using Microsoft.Extensions.Logging;
using PresenterApp.Services;
using PresenterApp.ViewModels;
using CommunityToolkit.Maui;
using PresenterApp.Views;

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

            builder.Services.AddSingleton<MainPage>();
            // Trang Bảng điều khiển
            builder.Services.AddSingleton<ManagementDashboardPage>();
            builder.Services.AddSingleton<ManagementDashboardViewModel>();

            // Các trang chỉnh sửa (dùng Transient vì chúng được tạo/hủy thường xuyên)
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


            return builder.Build();
        }
    }
}