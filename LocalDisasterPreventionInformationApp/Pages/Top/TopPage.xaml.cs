using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Top;

//ContentPageを継承
public partial class TopPage : ContentPage {
    public TopPage() {
        InitializeComponent();
        //PageTitleを「トップページ」にする
        (Shell.Current.BindingContext as AppShellViewModel).PageTitle = "トップページ";
    }
}