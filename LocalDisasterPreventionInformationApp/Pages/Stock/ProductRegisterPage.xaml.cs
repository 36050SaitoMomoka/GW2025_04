using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Stock;

//ContentPage‚ğŒp³
public partial class ProductRegisterPage : ContentPage {
    public ProductRegisterPage() {
        InitializeComponent();
        //PageTitle‚ğu”õ’~ŠÇ—v‚É‚·‚é
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "”õ’~ŠÇ—";
        }
    }
}