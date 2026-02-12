using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Hazard;

//ContentPageを継承
public partial class HazardMapPage : ContentPage {
    public HazardMapPage() {
        InitializeComponent();
        //PageTitleを「ハザードマップ」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "ハザードマップ";
        }
    }
}
