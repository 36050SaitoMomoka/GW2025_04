using Microsoft.Extensions.Logging;
using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages;
using LocalDisasterPreventionInformationApp.Pages.Hazard;
using LocalDisasterPreventionInformationApp.Pages.Stock;
using LocalDisasterPreventionInformationApp.Pages.Top;
using LocalDisasterPreventionInformationApp.Services;
using LocalDisasterPreventionInformationApp.Pages.Register;
using LocalDisasterPreventionInformationApp.Pages.Top;

namespace LocalDisasterPreventionInformationApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            //DIの注入
            builder.Services.AddSingleton<AppDatabase>(s => {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db3");
                var db = new AppDatabase(dbPath);

                return db;
            });

            builder.Services.AddHttpClient<ShelterService>();
            builder.Services.AddSingleton<AppShell>();

            //DBを使うページ登録
            builder.Services.AddTransient<HazardMapPage>();
            builder.Services.AddTransient<ProductRegisterPage>();
            builder.Services.AddTransient<StockPage>();
            builder.Services.AddTransient<TopPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
