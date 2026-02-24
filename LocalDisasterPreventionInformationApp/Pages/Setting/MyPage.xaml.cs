using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPageを継承
public partial class MyPage : ContentPage, INotifyPropertyChanged {
    private readonly AppDatabase _db;

    //翻訳用に仮追加
    public AppShellViewModel ShellVM => Shell.Current.BindingContext as AppShellViewModel;

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
        set {
            if (string.IsNullOrWhiteSpace(value)) {
                _phone = "---";
            } else {
                _phone = NormalizePhone(value);
            }
            OnPropertyChanged();
        }
    }

    private string _address;
    public string Address {
        get => _address;

        set { _address = value; OnPropertyChanged(); }
    }

    public MyPage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        BindingContext = this;
        Inner.BindingContext = this;

        //翻訳のために仮追加
        Resources["ShellVM"] = ShellVM;

        LoadData();

        //PageTitleを「マイページ」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = vm.Header_MyPage;

            // 言語切り替え時にも Picker を更新
            vm.PropertyChanged += (s, e) => {
                if (e.PropertyName == null || e.PropertyName == "SelectedLanguage") {
                    vm.PageTitle = vm.Header_MyPage;
                }
            };
        }
    }

    protected override async void OnAppearing() {
        base.OnAppearing();
        var addresses = await _db.GetAddressesAsync();
        LoadData();
    }

    private async void LoadData() {
        var user = await _db.GetUserAsync();

        var addresses = await _db.GetAddressesAsync();

        UserName = user.Name;
        Furigana = user.Furigana;
        Email = string.IsNullOrWhiteSpace(user.Email) ? "---" : user.Email;
        Phone = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "---" : user.PhoneNumber;

        AddressContainer.Children.Clear();

        foreach (var addr in addresses) {
            var stack = new VerticalStackLayout {
                Spacing = 4,
                Margin = new Thickness(0,10,0,10)
            };

            // 種別（1行目）
            stack.Children.Add(new Label {
                Text = $"【{addr.AddressType}】",
                FontSize = 17,
                TextColor = Colors.Black
            });

            // 住所（2行目）
            stack.Children.Add(new Label {
                Text = addr.Address,
                FontSize = 17,
                TextColor = Colors.Black,
                LineBreakMode = LineBreakMode.WordWrap
            });

            AddressContainer.Children.Add(stack);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // 編集ボタン
    private async void OnEditButtonClick(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("editprofilepage");
    }

    private string NormalizePhone(string input) {
        if (string.IsNullOrWhiteSpace(input)) return "";

        string num = input.Normalize(NormalizationForm.FormKC);
        num = new string(num.Where(char.IsDigit).ToArray());

        if (num.StartsWith("0120") && num.Length == 10)
            return $"{num[..4]}-{num.Substring(4, 3)}-{num.Substring(7)}";

        if (num.StartsWith("0570") && num.Length == 10)
            return $"{num[..4]}-{num.Substring(4, 2)}-{num.Substring(6)}";

        if ((num.StartsWith("090") || num.StartsWith("080") ||
             num.StartsWith("070") || num.StartsWith("060")) && num.Length == 11)
            return $"{num[..3]}-{num.Substring(3, 4)}-{num.Substring(7)}";

        if (num.StartsWith("050") && num.Length == 11)
            return $"{num[..3]}-{num.Substring(3, 4)}-{num.Substring(7)}";

        string[] area4 = { "0276", "0285", "0297", "0299", "0466", "0476", "0479" };
        if (num.Length == 10 && area4.Any(a => num.StartsWith(a)))
            return $"{num[..4]}-{num.Substring(4, 2)}-{num.Substring(6)}";

        string[] area3 = { "027", "028", "029", "03", "04", "045", "046", "047", "048", "049" };
        if (num.Length == 10 && area3.Any(a => num.StartsWith(a)))
            return $"{num[..3]}-{num.Substring(3, 3)}-{num.Substring(6)}";

        if (num.Length == 10 && (num.StartsWith("03") || num.StartsWith("06")))
            return $"{num[..2]}-{num.Substring(2, 4)}-{num.Substring(6)}";

        return num;
    }
}