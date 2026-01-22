using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPageを継承
public partial class LanguagePage : ContentPage {
    public LanguagePage() {
        InitializeComponent();
        //PageTitleを「言語選択」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "言語選択";
        }
    }
}