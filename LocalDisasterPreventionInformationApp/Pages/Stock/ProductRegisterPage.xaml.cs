using LocalDisasterPreventionInformationApp.Database;

namespace LocalDisasterPreventionInformationApp.Pages.Stock;

public partial class ProductRegisterPage : ContentPage
{

	private readonly AppDatabase _db;

	public ProductRegisterPage(AppDatabase db)
	{
		InitializeComponent();
		_db = db;
	}
}