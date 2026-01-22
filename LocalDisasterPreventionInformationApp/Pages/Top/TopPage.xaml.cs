using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Top;

//ContentPageを継承
public partial class TopPage : ContentPage {

    private readonly AppDatabase _db;

    public TopPage(AppDatabase db) {
        InitializeComponent();
        _db = db;
    }

    //PageTitleを「トップページ」にする
    protected override void OnAppearing() {
        base.OnAppearing();

        //前のページタイトルが残る問題を防ぐ
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "トップページ";
        }
    }
}