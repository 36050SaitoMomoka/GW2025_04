using LocalDisasterPreventionInformationApp.Database;
using LocalDisasterPreventionInformationApp.Services;
using System.Diagnostics;

namespace LocalDisasterPreventionInformationApp.Pages.Startup;

public partial class StartupPage : ContentPage {
    private readonly AppDatabase _db;
    private readonly ShelterService _shelterService;

    private static readonly Stopwatch _globalWatch = Stopwatch.StartNew();

    public StartupPage(AppDatabase db, ShelterService shelterService) {
        _db = db;
        _shelterService = shelterService;

        // 背景色を先に適用（青いチラつき防止）
        this.BackgroundColor = Colors.White;

        Debug.WriteLine($"[Startup] コンストラクタ開始: {_globalWatch.ElapsedMilliseconds} ms");
        InitializeComponent();
        Debug.WriteLine($"[Startup] コンストラクタ終了: {_globalWatch.ElapsedMilliseconds} ms");

        // ★ ゲージの初期描画を完全に隠す（青いゲージが見える問題の対策）
        GaugePath.Opacity = 0;                 // ゲージ全体を透明にする
        ArcSegment.Point = new Point(75, 10);  // 0% の位置にしない（初期描画を防ぐ）
        ArcSegment.IsLargeArc = false;
    }

    private async void ContentPage_Loaded(object sender, EventArgs e) {
        Debug.WriteLine($"[Startup] Loaded 発火: {_globalWatch.ElapsedMilliseconds} ms");

        await Task.Delay(50); // 描画安定待ち

        await RunStartupProcessAsync();
    }

    private async Task RunStartupProcessAsync() {
        Debug.WriteLine($"[Startup] 初期処理開始: {_globalWatch.ElapsedMilliseconds} ms");

        // 内部処理の開始時間
        var totalSw = Stopwatch.StartNew();

        var sw = new Stopwatch();

        // DB 初期化
        sw.Restart();
        await _db.InitializeAsync();
        sw.Stop();
        Debug.WriteLine($"[Startup] DB 初期化: {sw.ElapsedMilliseconds} ms");

        // 避難所データ読み込み
        sw.Restart();
        var shelters = await _db.GetSheltersAsync();
        sw.Stop();
        Debug.WriteLine($"[Startup] 避難所データ読み込み: {sw.ElapsedMilliseconds} ms");

        // 位置情報取得
        sw.Restart();
        var location = await Geolocation.GetLocationAsync();
        sw.Stop();
        Debug.WriteLine($"[Startup] 位置情報取得: {sw.ElapsedMilliseconds} ms");

        // TEST（AppShell から移動）
        await RunTestDataChecks();

        Debug.WriteLine($"[Startup] 初期処理完了: {_globalWatch.ElapsedMilliseconds} ms");

        // 内部処理の合計時間
        totalSw.Stop();
        long totalMs = totalSw.ElapsedMilliseconds;

        // 最低アニメーション時間（速すぎると不自然）
        if (totalMs < 1500)
            totalMs = 1500;

        // ★ ゲージを表示（初期描画を隠していたのでここで表示）
        MainThread.BeginInvokeOnMainThread(() => {
            GaugePath.Opacity = 1;
        });

        // 0 → 100% のアニメーション
        await AnimateProgressTo100(totalMs);

        // フェードアウト
        await Task.Delay(200);
        await this.FadeTo(0, 600);

        Debug.WriteLine($"[TEST] Shelter 件数： {shelters.Count}");

        //TEST
        Preferences.Set("IsRegistered", false);

        // 遷移
        bool isRegistered = Preferences.Get("IsRegistered", false);

        if (!isRegistered)
            // RegisterPageへ
            await Shell.Current.GoToAsync("//RegisterPage");
        else
            // 会員登録済み → TopPageへ
            await Shell.Current.GoToAsync("//TopPage");
    }

    // 内部処理の合計時間に合わせて 0 → 100% を滑らかに進める
    private async Task AnimateProgressTo100(long durationMs) {
        double percent = 0;
        double step = 100.0 / (durationMs / 16.0); // 60fps

        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < durationMs) {
            percent += step;
            if (percent > 100) percent = 100;

            UpdateProgress(percent);
            await Task.Delay(16);
        }

        UpdateProgress(100);
    }

    // TEST（AppShell から移動）
    private async Task RunTestDataChecks() {
        var products = await _db.GetProductsAsync();
        Debug.WriteLine($"[TEST] Product 件数： {products.Count}");
        foreach (var p in products)
            Debug.WriteLine($"[TEST] Product: ID={p.ProductId}, Name={p.Name}, Category={p.Category}");

        var stocks = await _db.GetStocksAsync();
        Debug.WriteLine($"[TEST] Stock 件数： {stocks.Count}");
        foreach (var s in stocks)
            Debug.WriteLine($"[TEST] Stock: ID={s.StockId}, pID={s.ProductId}, Date={s.ExpirationDate}, Qua={s.Quantity}");
    }

    // ゲージ更新
    public void UpdateProgress(double percent) {
        MainThread.BeginInvokeOnMainThread(() => {
            PercentLabel.Text = $"{percent:F0}％";

            double angle = percent * 3.6;
            double radians = (Math.PI / 180) * (angle - 90);

            double x = 75 + 65 * Math.Cos(radians);
            double y = 75 + 65 * Math.Sin(radians);

            ArcSegment.Point = new Point(x, y);
            ArcSegment.IsLargeArc = angle > 180;
        });
    }
}