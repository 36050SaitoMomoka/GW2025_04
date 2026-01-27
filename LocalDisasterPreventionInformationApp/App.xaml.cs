using Microsoft.Maui.Storage;
using LocalDisasterPreventionInformationApp.Pages.Register;
using LocalDisasterPreventionInformationApp.Services;

namespace LocalDisasterPreventionInformationApp {
    public partial class App : Application {

        private readonly AppShell _appShell;

        public App(AppShell appShell) {
            InitializeComponent();
            _appShell = appShell;
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            return new Window(_appShell);
        }
    }
}