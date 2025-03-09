using Microsoft.Extensions.Logging;
using PlasticQC.Services;
using PlasticQC.ViewModels;
using PlasticQC.Views;
using CommunityToolkit.Maui;

namespace PlasticQC
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

            // Services - use singleton for persistent services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<NavigationService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<ExportService>();
            builder.Services.AddSingleton<ProductService>();
            builder.Services.AddSingleton<RecordService>();

            // ViewModels - register as transient
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<AdminPanelViewModel>();
            builder.Services.AddTransient<UserManagementViewModel>();
            builder.Services.AddTransient<ProductManagementViewModel>();
            builder.Services.AddTransient<DailyEntryViewModel>();
            builder.Services.AddTransient<RecordsViewModel>();
            builder.Services.AddTransient<ComparisonViewModel>();

            // Views - register as transient
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AdminPanelPage>();
            builder.Services.AddTransient<UserManagementPage>();
            builder.Services.AddTransient<ProductManagementPage>();
            builder.Services.AddTransient<DailyEntryPage>();
            builder.Services.AddTransient<RecordsPage>();
            builder.Services.AddTransient<ComparisonPage>();

#if DEBUG
            builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Debug);
#endif

            return builder.Build();
        }
    }
}