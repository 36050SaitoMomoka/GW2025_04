using LocalDisasterPreventionInformationApp.Pages.Base;

namespace LocalDisasterPreventionInformationApp.Pages.Disaster;

// ContentPage を継承
public partial class DisasterPage : ContentPage {
    public DisasterPage() {
        InitializeComponent();

        // PageTitle を「災害情報」にする
        if (Shell.Current.BindingContext is AppShellViewModel vm) {
            vm.PageTitle = "災害情報";
        }

        //仮実行用
        LocationLabel.Text = "東京都千代田区";
        DisasterLabel.Text = "地震";
        ScaleLabel.Text = "M4.5";
        CommentLabel.Text = "避難の必要はありません";
        NationwideLabel.Text = "全国の災害情報を表示";
    }
}