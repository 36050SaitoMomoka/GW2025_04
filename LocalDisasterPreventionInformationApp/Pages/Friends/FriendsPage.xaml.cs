using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Friends;

//ContentPage‚ğŒp³
public partial class FriendsPage : ContentPage {
    public FriendsPage() {
        InitializeComponent();
        //PageTitle‚ğu—F’Bˆê——v‚É‚·‚é
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "—F’Bˆê——";
        }
    }
}