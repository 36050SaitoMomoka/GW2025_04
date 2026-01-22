using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Disaster;

//ContentPage‚ğŒp³
public partial class DisasterPage : ContentPage {
    public DisasterPage() {
        InitializeComponent();
        //PageTitle‚ğuĞŠQî•ñv‚É‚·‚é
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "ĞŠQî•ñ";
        }
    }
}