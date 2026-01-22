using System.ComponentModel;
using System.Windows.Input;

namespace LocalDisasterPreventionInformationApp {
    public class AppShellViewModel : INotifyPropertyChanged {
        // ニュースとして表示するテキスト一覧（後で変更）
        private readonly List<string> newsItems = new()
        {
            "群馬県で震度5弱の地震が発生しました。",
            "太田市内の避難所が開設されました。",
            "気象庁が余震に注意を呼びかけています。"
        };

        private int newsIndex = 0;      // 現在表示しているニュースのインデックス

        private string newsText;
        public string NewsText {
            get => newsText;
            set {
                if (newsText == value) return;
                newsText = value;
                OnPropertyChanged(nameof(NewsText));        // UIに変更を通知
            }
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
            RouteSearchCommand = new Command(async() => {
                await Shell.Current.GoToAsync("///TopPage");
            });

            // ニュースの初期化
            NewsText = newsItems[0];
            StartNewsCycle();
        }

        // ニュース切り替え開始
        private void StartNewsCycle() {
            RunNewsAnimation();
        }

        // ５秒ごとにニュースを切り替える
        private void RunNewsAnimation() {
            Dispatcher.GetForCurrentThread().DispatchDelayed(TimeSpan.FromSeconds(5), () => {
                newsIndex = (newsIndex + 1) % newsItems.Count;
                NewsText = newsItems[newsIndex];

                RunNewsAnimation();
            });
        }

        // プロパティ変更通知
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}