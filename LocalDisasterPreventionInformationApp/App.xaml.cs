using Microsoft.Maui.Storage;
using LocalDisasterPreventionInformationApp.Pages.Register;
using LocalDisasterPreventionInformationApp.Services;
using LocalDisasterPreventionInformationApp.Database;

namespace LocalDisasterPreventionInformationApp {
    public partial class App : Application {
        public static List<string> PrefectureDictionary { get; private set; } = new();
        public static List<string> CityDictionary { get; private set; } = new();

        private readonly AppShell _appShell;
        private readonly AppDatabase _db;

        public App(AppShell appShell,AppDatabase db) {
            InitializeComponent();
            _appShell = appShell;
            _db = db;

            Task.Run(LoadAddressDictionaryAsync).Wait();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            return new Window(_appShell);
        }

        private async Task LoadAddressDictionaryAsync() {
            var shelters = await _db.GetSheltersAsync();

            PrefectureDictionary = shelters.Select(s => s.Prefecture)
                                        .Distinct()
                                        .ToList();

            CityDictionary = shelters.Select(s => s.City)
                                        .Distinct()
                                        .ToList();
        }
    }
}