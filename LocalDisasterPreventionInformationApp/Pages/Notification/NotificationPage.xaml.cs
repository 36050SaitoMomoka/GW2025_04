using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Notification;

//ContentPageを継承
public partial class NotificationPage : ContentPage {
    public NotificationPage() {
        InitializeComponent();
        //PageTitleを「通知」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "通知";
        }
    }
}