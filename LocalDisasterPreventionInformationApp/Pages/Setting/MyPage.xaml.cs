using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPageを継承
public partial class MyPage : ContentPage, INotifyPropertyChanged {
    private readonly AppDatabase _db;

    private string _userName;
    public string UserName {
        get => _userName;
        set { _userName = value; OnPropertyChanged(); }
    }

    private string _furigana;
    public string Furigana {
        get => _furigana;
        set { _furigana = value; OnPropertyChanged(); }
    }

    private string _email;
    public string Email {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    private string _phone;
    public string Phone {
        get => _phone;
        set { _phone = value; OnPropertyChanged(); }
    }

    private string _address;
    public string Address {
        get => _address;
        set { _address = value; OnPropertyChanged(); }
    }

    public MyPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        BindingContext = Shell.Current.BindingContext;

        Inner.BindingContext = this;

        LoadData();

        //PageTitleを「マイページ」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "マイページ";
        }
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        LoadData();
    }

    private async void LoadData() {
        var user = await _db.GetUserAsync();
        var addresses = await _db.GetAddressesAsync();
        var home = addresses.FirstOrDefault();

        UserName = user.Name;
        Furigana = user.Furigana;
        Email = user.Email;
        Phone = user.PhoneNumber;
        Address = home.Address;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // 編集ボタン
    private async void OnEditButtonClick(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("editprofilepage");
    }
}