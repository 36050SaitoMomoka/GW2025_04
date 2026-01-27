using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Stock;

//ContentPageを継承
public partial class ProductRegisterPage : ContentPage {
    public ProductRegisterPage() {
        InitializeComponent();
        //PageTitleを「備蓄管理」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "備蓄管理";
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("..");        //１つ前のページに戻る
    }
}