using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPageを継承
public partial class MyPage : ContentPage {
    private readonly AppDatabase _db;

    public MyPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        //PageTitleを「マイページ」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "マイページ";
        }
    }
}