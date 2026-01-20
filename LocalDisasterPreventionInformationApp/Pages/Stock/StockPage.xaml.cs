using LocalDisasterPreventionInformationApp.Database;

namespace LocalDisasterPreventionInformationApp.Pages.Stock;

public partial class StockPage : ContentPage
{

	private readonly AppDatabase _db;

	public StockPage(AppDatabase db)
	{
		InitializeComponent();
		_db = db;
	}
}