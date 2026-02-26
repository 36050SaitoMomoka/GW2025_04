using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Pages.Base;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LocalDisasterPreventionInformationApp.Pages.Setting;//test

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
            vm.PageTitle = vm.Header_Edit;

            // 言語切り替え時にも Picker を更新
            vm.PropertyChanged += (s, e) => {
                if (e.PropertyName == null || e.PropertyName == "SelectedLanguage") {
                    vm.PageTitle = vm.Header_Edit;
                }
            };
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
            var typeEntry = row.Children.OfType<Grid>().First().Children.OfType<Entry>().First();
            var addressEntry = row.Children.OfType<Grid>().First().Children.OfType<Entry>().Last();

            typeEntry.Text = addr.AddressType;
            addressEntry.Text = addr.Address;

            row.BindingContext = addr;

            var dummyZip = new Entry { Text = "" };
            var dummyAuto = new Entry { Text = "" };

            // ★ 既存住所行も _addressRows に登録する（null を入れない）
            _addressRows.Add((
                ZipEntry: dummyZip,
                TypeEntry: typeEntry,
                AutoAddressEntry: dummyAuto,
                AddressLineEntry: addressEntry
            ));


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
            RowDefinitions =
            {
            new RowDefinition { Height = GridLength.Auto }, // 住所種別
            new RowDefinition { Height = GridLength.Auto }, // 住所 + ＋
            new RowDefinition { Height = GridLength.Auto }  // 区切り線
        },
            ColumnDefinitions =
            {
            new ColumnDefinition { Width = GridLength.Auto }, // inner（住所種別＋住所）
            new ColumnDefinition { Width = GridLength.Auto }  // -ボタン
        }
        };

        var typeEntry = new Entry { FontSize = 26, WidthRequest = 650 };
        var addressEntry = new Entry { FontSize = 26, WidthRequest = 650 };

        // inner（住所種別 → 住所）
        var inner = new Grid {
            RowDefinitions =
            {
            new RowDefinition { Height = GridLength.Auto }, // 住所種別
            new RowDefinition { Height = GridLength.Auto }  // 住所
        },
            ColumnDefinitions =
            {
            new ColumnDefinition { Width = 650 } // 指定幅
        }
        };

        inner.Add(typeEntry, 0, 0);   // 住所種別
        inner.Add(addressEntry, 0, 1); // 住所

        // inner を左側に配置（2行分を占有）
        grid.Add(inner, 0, 0);
        Grid.SetRowSpan(inner, 2);

        // ★ マイナスボタン（住所セット削除）
        var removeButton = new Button {
            Text = "-",
            FontSize = 26,
            WidthRequest = 45,
            HeightRequest = 45,
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            CornerRadius = 10,
            Margin = new Thickness(5, 15, 0, 0)
        };
        removeButton.Clicked += OnRemoveAddressClicked;

        // 住所セットの右側に配置
        grid.Add(removeButton, 1, 0);

        // 区切り線（住所エリアだけ）
        var line = new BoxView {
            HeightRequest = 2,
            BackgroundColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Fill
        };
        grid.Add(line, 0, 2);
        Grid.SetColumnSpan(line, 2);

        return grid;
    }

    //新しい住所行を作るメソッド
    private Grid CreateAddressRow() {
        var grid = new Grid {
            RowDefinitions =
            {
            new RowDefinition { Height = GridLength.Auto }, // 種類
            new RowDefinition { Height = GridLength.Auto }, // 郵便番号
            new RowDefinition { Height = GridLength.Auto }, // 自動住所
            new RowDefinition { Height = GridLength.Auto }  // 番地 + ＋
        },
            ColumnDefinitions =
            {
            new ColumnDefinition { Width = 650 },          // 入力欄（左端揃え）
            new ColumnDefinition { Width = GridLength.Auto } // ＋ボタン
        }
        };

        // ① 住所種類
        var typeEntry = new Entry {
            Placeholder = "住所種類（例：自宅・実家・職場）",
            FontSize = 20,
            TextColor = Colors.Black,
            WidthRequest = 650
        };
        grid.Add(typeEntry, 0, 0); // ★ col0 のみに置く

        // ② 郵便番号
        var zipEntry = new Entry {
            Placeholder = "郵便番号（7桁）",
            Keyboard = Keyboard.Numeric,
            FontSize = 20,
            Margin = new Thickness(0, 5, 0, 5),
            WidthRequest = 650
        };
        zipEntry.TextChanged += OnZipChanged;
        grid.Add(zipEntry, 0, 1); // ★ col0 のみに置く

        // ③ 自動住所
        var autoAddressEntry = new Entry {
            Placeholder = "都道府県 市区町村 町域（自動入力）",
            FontSize = 20,
            IsReadOnly = true,
            TextColor = Colors.Black,
            WidthRequest = 650
        };
        grid.Add(autoAddressEntry, 0, 2); // ★ col0 のみに置く

        // ④ 番地
        var addressLineEntry = new Entry {
            Placeholder = "番地（例：1-1）",
            FontSize = 20,
            TextColor = Colors.Black,
            WidthRequest = 650
        };
        grid.Add(addressLineEntry, 0, 3); // ★ col0 のみに置く

        // ★ マイナスボタン（住所セット削除）
        var removeButton = new Button {
            Text = "-",
            FontSize = 20,
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            CornerRadius = 10,
            WidthRequest = 45,
            HeightRequest = 45,
            Margin = new Thickness(5, 0, 0, 0)
        };
        removeButton.Clicked += OnRemoveAddressClicked;

        // 番地の横に配置
        grid.Add(removeButton, 1, 3);

        // 保持
        _addressRows.Add((zipEntry, typeEntry, autoAddressEntry, addressLineEntry));

        return grid;
    }


    private async void OnAddAddressClicked(object sender, EventArgs e) {
        var button = sender as Button;
        if (button == null) return;

        // 住所ラベル横の＋ボタンは Grid.Row=16 にあるので消さない
        if (button.Parent is Grid parentGrid && Grid.GetRow(button) == 16) {
            // 新しい住所行を追加するだけ
            var newRow = CreateAddressRow();
            AddressContainer.Add(newRow);
            return;
        }

        // それ以外（住所行の＋ボタン）は消す
        var grid = button.Parent as Grid;
        grid?.Children.Remove(button);


        var rowFromAddress = CreateAddressRow();
        AddressContainer.Add(rowFromAddress);
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

    private async void OnRemoveAddressClicked(object sender, EventArgs e) {
        if (sender is not Button btn) return;

        bool confirm = await DisplayAlert(
            "確認",
            "この住所を削除しますか？",
            "削除する",
            "キャンセル"
        );

        if (!confirm)
            return;

        if (btn.Parent is Grid row) {
            // UI から削除
            AddressContainer.Children.Remove(row);

            // _addressRows から削除（新規・既存どちらも）
            var target = _addressRows.FirstOrDefault(r =>
                r.ZipEntry?.Parent?.Parent == row ||
                r.TypeEntry?.Parent?.Parent == row ||
                r.AutoAddressEntry?.Parent?.Parent == row ||
                r.AddressLineEntry?.Parent?.Parent == row
            );

            if (target.TypeEntry != null)
                _addressRows.Remove(target);

            // ★ 既存住所行なら DB から削除（UserAddress をそのまま渡す）
            if (row.BindingContext is Models.UserAddress addr) {
                await _db.DeleteAddressAsync(addr);
            }
        }
    }
}