using Microsoft.Extensions.Logging;
using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages;
using LocalDisasterPreventionInformationApp.Pages.Hazard;
using LocalDisasterPreventionInformationApp.Pages.Stock;

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
                return new AppDatabase(dbPath);
            });

            //後で追加
            //builder.Services.AddSingleton<ShelterServices>();

            //DBを使うページ登録
            builder.Services.AddSingleton<HazardMapPage>();
            builder.Services.AddSingleton<ProductRegisterPage>();
            builder.Services.AddSingleton<StockPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
