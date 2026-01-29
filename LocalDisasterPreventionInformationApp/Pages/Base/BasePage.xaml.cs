namespace LocalDisasterPreventionInformationApp.Pages.Base;

//共通レイアウトを提供するベースページ
public partial class BasePage : ContentView {
    public static readonly BindableProperty InnerContentProperty =
        BindableProperty.Create(
            nameof(InnerContent),
            typeof(View),
            typeof(BasePage),
            propertyChanged: OnInnerContentChanged);

    public View InnerContent {
        get => (View)GetValue(InnerContentProperty);
        set => SetValue(InnerContentProperty, value);
    }

    // InnerContentが変更されたら
    private static void OnInnerContentChanged(BindableObject bindable, object oldValue, object newValue) {
        var basePage = (BasePage)bindable;
        basePage.ContentArea.Content = (View)newValue;
    }

    public BasePage() {
        InitializeComponent();

        // Shell の BindingContext を引き継ぐ
        HeaderArea.BindingContext = Shell.Current.BindingContext;
        FooterArea.BindingContext = Shell.Current.BindingContext;

        // ページ側の BindingContext を BasePage に伝える
        this.BindingContextChanged += (s, e) => {
            ContentArea.BindingContext = this.BindingContext;
        };
    }
}