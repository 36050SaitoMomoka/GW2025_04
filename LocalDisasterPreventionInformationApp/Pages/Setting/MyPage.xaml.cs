using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPageを継承
public partial class MyPage : ContentPage {
    public MyPage() {
        InitializeComponent();
        //PageTitleを「マイページ」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "マイページ";
        }
    }
}