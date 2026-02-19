using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Globalization;
using LocalDisasterPreventionInformationApp.Resources.Strings;
using static System.Net.WebRequestMethods;
using LocalDisasterPreventionInformationApp.Pages.Top;

namespace LocalDisasterPreventionInformationApp {
    public class AppShellViewModel : INotifyPropertyChanged {
        // 言語一覧
        public List<string> LanguageList { get; } = new()
        {
            "　　日本語",
            "　　English",
            "　　한국어",
            "　　中文"
        };

        // 選択された言語
        private string selectedLanguage;
        public string SelectedLanguage {
            get => selectedLanguage;
            set {
                if (selectedLanguage == value) return;
                selectedLanguage = value;
                OnPropertyChanged();

                // 言語切り替え
                SetCulture(value);

                // 翻訳プロパティを更新
                OnPropertyChanged(null);
            }
        }

        // 言語を切り替えるメソッド
        private void SetCulture(string lang) {
            CultureInfo culture = lang switch {
                "　　English" => new CultureInfo("en"),
                "　　한국어" => new CultureInfo("ko"),
                "　　中文" => new CultureInfo("zh-Hans"),
                _ => new CultureInfo("ja")
            };

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // .resxに適用
            AppResources.Culture = culture;

            // UI更新
            OnPropertyChanged(nameof(PageTitle));
        }

        #region 翻訳プロパティ
        public string Btn_AddProduct => AppResources.Btn_AddProduct;
        public string Btn_Back => AppResources.Btn_Back;
        public string Btn_Edit => AppResources.Btn_Edit;
        public string Btn_Next => AppResources.Btn_Next;
        public string Btn_Register => AppResources.Btn_Register;
        public string Btn_Discard => AppResources.Btn_Discard;
        public string Btn_Confirm => AppResources.Btn_Confirm;

        public string Header_AppTitle => AppResources.Header_AppTitle;
        public string Header_Font => AppResources.Header_Font;
        public string Header_HazardMap => AppResources.Header_HazardMap;
        public string Header_MyPage => AppResources.Header_MyPage;
        public string Header_RouteSearch => AppResources.Header_RouteSearch;
        public string Header_Stock => AppResources.Header_Stock;
        public string Header_Top => AppResources.Header_Top;

        public string Menu_HazardMap => AppResources.Menu_HazardMap;
        public string Menu_Stock => AppResources.Menu_Stock;
        public string Menu_Top => AppResources.Menu_Top;

        public string Reg_Address => AppResources.Reg_Address;
        public string Reg_AutoFill => AppResources.Reg_AutoFill;
        public string Reg_AutoFromZip => AppResources.Reg_AutoFromZip;
        public string Reg_Building => AppResources.Reg_Building;
        public string Reg_City => AppResources.Reg_City;
        public string Reg_Confirm => AppResources.Reg_Confirm;
        public string Reg_Email => AppResources.Reg_Email;
        public string Reg_Empty => AppResources.Reg_Empty;
        public string Reg_Furigana => AppResources.Reg_Furigana;
        public string Reg_FuriganaHint => AppResources.Reg_FuriganaHint;
        public string Reg_Name => AppResources.Reg_Name;
        public string Reg_Optional => AppResources.Reg_Optional;
        public string Reg_Phone => AppResources.Reg_Phone;
        public string Reg_Prefecture => AppResources.Reg_Prefecture;
        public string Reg_Town => AppResources.Reg_Town;
        public string Reg_Welcome => AppResources.Reg_Welcome;
        public string Reg_Zip => AppResources.Reg_Zip;
        public string Reg_FuriganaLabel => AppResources.Reg_FuriganaLabel;
        public string Reg_AddressType => AppResources.Reg_AddressType;

        public string Stock_Category => AppResources.Stock_Category;
        public string Stock_CategoryOrder => AppResources.Stock_CategoryOrder;
        public string Stock_Delete => AppResources.Stock_Delete;
        public string Stock_Expire => AppResources.Stock_Expire;
        public string Stock_ExpireOrder => AppResources.Stock_ExpireOrder;
        public string Stock_NameOrder => AppResources.Stock_NameOrder;
        public string Stock_Others => AppResources.Stock_Others;
        public string Stock_ProductName => AppResources.Stock_ProductName;
        public string Stock_Quantity => AppResources.Stock_Quantity;
        public string Stock_Sort => AppResources.Stock_Sort;

        public string Top_ShelterList => AppResources.Top_ShelterList;

        public string Hazard_Earthquake => AppResources.Hazard_Earthquake;
        public string Hazard_Tsunami => AppResources.Hazard_Tsunami;
        public string Hazard_FloodLandslide => AppResources.Hazard_FloodLandslide;
        #endregion


        // ページタイトル
        private string pageTitle;
        public string PageTitle {
            get => pageTitle;
            set {
                if (pageTitle == value) return;
                pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        // ヘッダーのボタン用
        public ICommand MyPageCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand RouteSearchCommand { get; }
        public Command DrivingCommand => new(() => CurrentRouteMode = "driving");
        public Command CyclingCommand => new(() => CurrentRouteMode = "bicycling");
        public Command WalkingCommand => new(() => CurrentRouteMode = "walking");
        public ICommand ClearRouteCommand { get; private set; }

        public event Action<string> RouteModeChanged;

        public AppShellViewModel() {

            // 前回選んだ言語を復元（初期値：日本語）
            SelectedLanguage = Preferences.Get("SelectedLanguage", "　　日本語");
            SetCulture(SelectedLanguage);

            //マイページへ
            MyPageCommand = new Command(async () => {
                await Shell.Current.GoToAsync("mypage");
            });

            //ハンバーガーメニューを開く
            OpenMenuCommand = new Command(() => {
                Shell.Current.FlyoutIsPresented = true;
            });

            //ルート検索をする（トップページへ遷移しルートを検索する）
            RouteSearchCommand = new Command(async () => {
                // トップページにいる場合は直接呼ぶ
                var topPage = Shell.Current.CurrentPage as TopPage;
                if(topPage != null) {
                    await topPage.RouteToNearestShelterAsync("driving");
                } else {
                    await Shell.Current.GoToAsync("//TopPage?route=1");
                }
            });

            // ルート削除
            ClearRouteCommand = new Command(async () => {
                var topPage = Shell.Current.CurrentPage as TopPage;
                if (topPage != null) {
                    await topPage.ClearRouteAsync();
                }
            });
        }

        // プロパティ変更通知
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // ルート検索用
        public List<string> RouteModes { get; } = new() { "driving", "walking", "bicycling" };
        private string _currentRouteMode = "driving";
        public string CurrentRouteMode {
            get => _currentRouteMode;
            set {
                if (_currentRouteMode != value) {
                    _currentRouteMode = value;
                    RouteModeChanged?.Invoke(value);
                }
            }
        }
    }
}