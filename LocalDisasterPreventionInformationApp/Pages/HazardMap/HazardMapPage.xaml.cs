using LocalDisasterPreventionInformationApp.Database;

namespace LocalDisasterPreventionInformationApp.Pages.Hazard;

public partial class HazardMapPage : ContentPage
{

	private readonly AppDatabase _db;

	public HazardMapPage(AppDatabase db)
	{
		InitializeComponent();
		_db = db;
	}
}