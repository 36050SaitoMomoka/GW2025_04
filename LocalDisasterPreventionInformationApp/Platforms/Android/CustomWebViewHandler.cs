#if ANDROID

using Microsoft.Maui.Handlers;

namespace LocalDisasterPreventionInformationApp.Platforms.Android {
    public class CustomWebViewHandler : WebViewHandler {
        protected override void ConnectHandler(global::Android.Webkit.WebView platformView) {
            base.ConnectHandler(platformView);

            var settings = platformView.Settings;

            settings.JavaScriptEnabled = true;
            settings.DomStorageEnabled = true;

            settings.AllowFileAccessFromFileURLs = true;
            settings.AllowUniversalAccessFromFileURLs = true;

            // 外部リソース（OSMタイル）を許可
            settings.MixedContentMode = global::Android.Webkit.MixedContentHandling.AlwaysAllow;

            //ここから追加
            platformView.SetWebViewClient(new global::Android.Webkit.WebViewClient());
            platformView.ClearHistory();
            settings.AllowContentAccess = true;
            settings.DatabaseEnabled = true;
            settings.MediaPlaybackRequiresUserGesture = false;

            //キャッシュを無効化
            settings.CacheMode = global::Android.Webkit.CacheModes.NoCache;
            platformView.ClearCache(true);
        }
    }
}
#endif