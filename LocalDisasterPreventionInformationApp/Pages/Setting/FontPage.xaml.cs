using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPageを継承
public partial class FontPage : ContentPage {
    public FontPage() {
        InitializeComponent();
        //PageTitleを「フォント選択」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "フォント選択";
        }
    }
}