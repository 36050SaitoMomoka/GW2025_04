using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;

//ContentPageを継承
public partial class EditProfilePage : ContentPage, INotifyPropertyChanged {
    private readonly AppDatabase _db;

    //住所行のEntryを保持するリスト
    private List<(Entry ZipEntry, Entry TypeEntry,
                  Entry AutoAddressEntry, Entry AddressLineEntry)> _addressRows = new();

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

    private string _addressType;
    public string AddressType {
        get => _addressType;
        set { _addressType = value; OnPropertyChanged(); }
    }

    public EditProfilePage(AppDatabase db) {
        InitializeComponent();
        _db = db;

        Inner.BindingContext = this;

        //最初の住所行をリストに登録
        var firstRow = CreateAddressRow();
        AddressContainer.Add(firstRow);

        LoadData();

        //PageTitleを「マイページ」にする
        var vm = Shell.Current.BindingContext as AppShellViewModel;
        if (vm != null) {
            vm.PageTitle = "マイページ";
        }
    }

    //住所から緯度経度を求める
    private async Task<(double? lat, double? lon)> GetLatLngFromAddress(string address) {
        try {
            string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "LocalDisasterPreventionApp/1.0");

            var json = await client.GetStringAsync(url);

            var results = JsonSerializer.Deserialize<List<NominatimResult>>(json);

            if (results != null && results.Count > 0) {
                double lat = double.Parse(results[0].lat, CultureInfo.InvariantCulture);
                double lon = double.Parse(results[0].lon, CultureInfo.InvariantCulture);
                return (lat, lon);
            }
        }
        catch {
            // 失敗したら null を返す
        }

        return (null, null);
    }

    public class NominatimResult {
        public string lat { get; set; }
        public string lon { get; set; }
    }

    protected override void OnAppearing() {
        base.OnAppearing();
        LoadData();
    }

    private async void LoadData() {
        var user = await _db.GetUserAsync();
        var addresses = await _db.GetAddressesAsync();

        UserName = user.Name;
        Furigana = user.Furigana;
        Email = user.Email;
        Phone = user.PhoneNumber;

        AddressContainer.Children.Clear();
        _addressRows.Clear();


        // DB の住所をすべて表示
        for (int i = 0; i < addresses.Count; i++) {
            var addr = addresses[i];
            var row = CreateExistingAddressRow();

            // 値をセット
            row.Children.OfType<Grid>().First().Children.OfType<Entry>().First().Text = addr.AddressType;
            row.Children.OfType<Grid>().First().Children.OfType<Entry>().Last().Text = addr.Address;

            // 最後の行以外は＋ボタンを消す
            if (i != addresses.Count - 1) {
                RemovePlusButton(row);
            }

            AddressContainer.Add(row);
        }

        // DB に住所が 0 件だった場合は空の行を 1 行追加
        if (addresses.Count == 0) {
            var emptyRow = CreateExistingAddressRow();
            AddressContainer.Add(emptyRow);
        }

    }

    private void RemovePlusButton(Grid row) {
        foreach (var child in row.Children) {
            if (child is Button) {
                row.Children.Remove(child);
                break;
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // zipcloud APIで住所取得
    private async Task<(string pref, string city, string town)> FetchAddressFromZip(string zip) {
        try {
            string url = $"https://zipcloud.ibsnet.co.jp/api/search?zipcode={zip}";
            using var client = new HttpClient();
            var json = await client.GetStringAsync(url);

            var result = JsonSerializer.Deserialize<ZipCloudResponse>(json);

            if(result?.results != null && result.results.Length > 0) {
                var r = result.results[0];
                return (r.address1, r.address2, r.address3);
            }
        }
        catch {
        }

        return (null, null, null);
    }

    // zipcloud API レスポンスモデル
    public class ZipCloudResponse {
        public ZipResult[] results { get; set; }
    }

    public class ZipResult {
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
    }

    //郵便番号から住所を作成
    private async void OnZipChanged(object sender, TextChangedEventArgs e) {

        if (sender is not Entry zipEntry) return;

        var row = _addressRows.FirstOrDefault(r => r.ZipEntry == zipEntry);
        if (row.ZipEntry == null) return;

        string zip = zipEntry.Text?.Trim();
        if (zip?.Length != 7) return;

        var (pref, city, town) = await FetchAddressFromZip(zip);
        if (pref == null) return;

        if (row.AutoAddressEntry != null) {
            row.AutoAddressEntry.Text = $"{pref} {city} {town}";
        }
    }


    //既存データ表示用
    private Grid CreateExistingAddressRow() {
        var grid = new Grid {
            ColumnDefinitions =
            {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = GridLength.Auto },
            }
        };

        var typeEntry = new Entry { WidthRequest = 100, FontSize = 26 };
        var addressEntry = new Entry { FontSize = 26 };

        grid.Add(new Label { Text = "住所", FontSize = 22 }, 0, 0);
        grid.Add(new Label { Text = "：", FontSize = 22 }, 1, 0);

        var inner = new Grid {
            ColumnDefinitions =
            {
            new ColumnDefinition { Width = GridLength.Auto },
            new ColumnDefinition { Width = 10 },
            new ColumnDefinition { Width = GridLength.Star }
        }
        };

        inner.Add(typeEntry, 0, 0);
        inner.Add(addressEntry, 2, 0);

        grid.Add(inner, 2, 0);

        var addButton = new Button {
            Text = "+",
            FontSize = 20,
            BackgroundColor = Color.FromArgb("#2196F3"),
            TextColor = Colors.White,
            CornerRadius = 10,
            Margin = new Thickness(5, 0, 0, 0)
        };
        addButton.Clicked += OnAddAddressClicked;

        grid.Add(addButton, 3, 0);

        return grid;
    }

    //新しい住所行を作るメソッド
    private Grid CreateAddressRow() {
        var grid = new Grid {
            RowDefinitions =
            {
            new RowDefinition { Height = GridLength.Auto }, // 種類
            new RowDefinition { Height = GridLength.Auto }, // 郵便番号
            new RowDefinition { Height = GridLength.Auto }, // 自動住所 + 番地 + ＋
        },
            ColumnDefinitions =
            {
            new ColumnDefinition { Width = GridLength.Star }, // 自動住所
            new ColumnDefinition { Width = GridLength.Star }, // 番地
            new ColumnDefinition { Width = GridLength.Auto }  // ＋ボタン
        }
        };

        // ① 住所種類
        var typeEntry = new Entry {
            Placeholder = "住所種類（例：自宅・実家・職場）",
            FontSize = 20,
            TextColor = Colors.Black
        };
        grid.Add(typeEntry, 0, 0);
        Grid.SetColumnSpan(typeEntry, 3);

        // ② 郵便番号
        var zipEntry = new Entry {
            Placeholder = "郵便番号（7桁）",
            Keyboard = Keyboard.Numeric,
            FontSize = 20,
            Margin = new Thickness(0, 5, 0, 5)
        };
        zipEntry.TextChanged += OnZipChanged;
        grid.Add(zipEntry, 0, 1);
        Grid.SetColumnSpan(zipEntry, 3);
        Grid.SetRowSpan(zipEntry, 1);

        // ③ 自動住所（都道府県 市区町村 町域）
        var autoAddressEntry = new Entry {
            Placeholder = "都道府県 市区町村 町域（自動入力）",
            FontSize = 20,
            IsReadOnly = true,
            TextColor = Colors.Black
        };
        grid.Add(autoAddressEntry, 0, 2);

        // ④ 番地（手入力）
        var addressLineEntry = new Entry {
            Placeholder = "番地（例：1-1）",
            FontSize = 20,
            TextColor = Colors.Black
        };
        grid.Add(addressLineEntry, 1, 2);

        // ⑤ ＋ボタン
        var addButton = new Button {
            Text = "+",
            FontSize = 20,
            BackgroundColor = Color.FromArgb("#2196F3"),
            TextColor = Colors.White,
            CornerRadius = 10,
            Margin = new Thickness(5, 0, 0, 0)
        };
        addButton.Clicked += OnAddAddressClicked;
        grid.Add(addButton, 2, 2);

        // ★ 新しい構造に合わせて保持
        _addressRows.Add((zipEntry, typeEntry, autoAddressEntry, addressLineEntry));

        return grid;
    }


    private async void OnAddAddressClicked(object sender, EventArgs e) {
        var button = sender as Button;
        if (button == null) return;

        var parentGrid = button.Parent as Grid;
        parentGrid.Remove(button);

        var newRow = CreateAddressRow();
        AddressContainer.Add(newRow);
    }

    //確定ボタン
    private async void OnSubmitClicked(object sender, EventArgs e) {
        //Userテーブルへ保存
        var user = new Models.User {
            Name = nameEntry.Text,
            Furigana = furiganaEntry.Text,
            PhoneNumber = phoneEntry.Text == "---" ? null : phoneEntry.Text,
            Email = emailEntry.Text == "---" ? null : emailEntry.Text
        };
        await _db.SaveUserAsync(user);

        // UserAddressテーブルへ保存（複数行対応）
        foreach (var row in _addressRows) {
            string zip = row.ZipEntry.Text;
            string type = row.TypeEntry.Text;
            string auto = row.AutoAddressEntry.Text;
            string line = row.AddressLineEntry.Text;

            if (string.IsNullOrWhiteSpace(auto) && string.IsNullOrWhiteSpace(line))
                continue;

            string fullAddress = $"{auto} {line}".Trim();

            var (lat, lon) = await GetLatLngFromAddress(fullAddress);

            var address = new Models.UserAddress {
                PostalCode = zip,
                AddressType = type,
                Address = fullAddress,
                Longitude = lon,
                Latitude = lat
            };

            await _db.AddAddressIfNotExistsAsync(address);
        }

        // TEST データが入ったか確認
        await _db.RunUserDataChecks();

        await Shell.Current.GoToAsync("//MyPage");
    }

    //キャンセルボタン
    private async void OnBackClicked(object sender, EventArgs e) {
        await Shell.Current.GoToAsync("//MyPage");
    }
}