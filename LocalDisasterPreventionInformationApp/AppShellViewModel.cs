using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using System.Xml.Linq;
using HtmlAgilityPack;
using static System.Net.WebRequestMethods;

namespace LocalDisasterPreventionInformationApp {
    public class AppShellViewModel : INotifyPropertyChanged {

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
        public ICommand FriendsCommand { get; }
        public ICommand NotificationCommand { get; }
        public ICommand SettingCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand RouteSearchCommand { get; }

        public AppShellViewModel() {
            // チャットページへ
            FriendsCommand = new Command(async () => {
                await Shell.Current.GoToAsync("///FriendsPage");
            });

            //通知ページへ
            NotificationCommand = new Command(async () => {
                await Shell.Current.GoToAsync("///NotificationPage");
            });

            //設定ページへ
            SettingCommand = new Command(async () => {
                await Shell.Current.GoToAsync("///SettingPage");
            });

            //ハンバーガーメニューを開く
            OpenMenuCommand = new Command(() => {
                Shell.Current.FlyoutIsPresented = true;
            });

            //ルート検索をする（トップページへ遷移しルートを検索する）
            RouteSearchCommand = new Command(async () => {
                await Shell.Current.GoToAsync("///TopPage");
            });

            _ = LoadWarnAsync();
        }

        // Yahooニュース検索結果を取得
        private async Task LoadWarnAsync() {
            try {
                newsItems = new List<NewsItem>();

                string url = "https://news.yahoo.co.jp/search?p=警報&ei=utf-8";

                using var http = new HttpClient();
                string html = await http.GetStringAsync(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Yahooニュース検索結果のタイトルを取得
                var nodes = doc.DocumentNode.SelectNodes("//a[contains{@href,'/articles/')]");

                if (nodes != null) {
                    foreach (var node in nodes) {
                        string title = node.InnerText.Trim();
                        string link = node.GetAttributeValue("href", "");

                        newsItems.Add(new NewsItem {
                            Title = title,
                            Link = link,
                        });
                    }
                }

                if (newsItems.Count > 0) {
                    newsIndex = 0;
                    newsText = newsItems[0].Title;
                    StartNewsCycle();
                } else {
                    newsText = "ニュースがありません。";
                }
            }
            catch (Exception ex) {
                newsText = $"取得エラー：{ex.Message}";
            }
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
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class NewsItem {
        public string Title { get; set; }
        public string Link { get; set; }
    }
}