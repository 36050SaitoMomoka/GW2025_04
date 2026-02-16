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

                // 言語を保存
                Preferences.Set("SelectedLanguage", selectedLanguage);
                SetCulture(selectedLanguage);
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
            OnPropertyChanged(nameof(NewsText));
        }

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

        // ニュース切り替え用
        private List<NewsItem> newsItems = new();
        private int newsIndex = 0;

        private string newsText;
        public string NewsText {
            get => newsText;
            set {
                newsText = value;
                OnPropertyChanged(nameof(NewsText));
            }
        }

        // ヘッダーのボタン用
        public ICommand FontCommand { get; }
        public ICommand MyPageCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand RouteSearchCommand { get; }

        public AppShellViewModel() {

            // 前回選んだ言語を復元（初期値：日本語）
            SelectedLanguage = Preferences.Get("SelectedLanguage", "　　日本語");
            SetCulture(SelectedLanguage);

            //フォント選択ページへ
            FontCommand = new Command(async () => {
                await Shell.Current.GoToAsync("fontpage");
            });

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
                await Shell.Current.GoToAsync("//TopPage?route=1");
            });

          //_ = LoadWarnAsync();
        }

        private void StartNewsCycle() => RunNewsAnimation();

        // ５秒ごとにニュースを切り替える
        private void RunNewsAnimation() {
            Dispatcher.GetForCurrentThread().DispatchDelayed(TimeSpan.FromSeconds(5), () => {
                newsIndex = (newsIndex + 1) % newsItems.Count;
                NewsText = newsItems[newsIndex].Title;

                RunNewsAnimation();
            });
        }

        // プロパティ変更通知
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class NewsItem {
        public string Title { get; set; }
        public string Link { get; set; }
    }
}