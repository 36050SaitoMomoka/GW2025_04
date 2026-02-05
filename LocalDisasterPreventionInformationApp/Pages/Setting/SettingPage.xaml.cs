using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPage‚ğŒp³
public partial class SettingPage : ContentPage {
    public SettingPage() {
        InitializeComponent();

        //PageTitle‚ğuİ’èv‚É‚·‚é
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "İ’è";
        }
    }

}