using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Friends;

//ContentPageを継承
public partial class SafetyListPage : ContentPage {
    public SafetyListPage() {
        InitializeComponent();
        //PageTitleを「安否確認一覧」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "安否確認一覧";
        }
    }
}