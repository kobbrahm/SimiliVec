using SimiliVec.MAUI.ViewModels;

namespace SimiliVec.MAUI;

public partial class MainPage : ContentPage
{
	private readonly SearchPageViewModel _viewModel;

	public MainPage(SearchPageViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

	private async void OnSearchClicked(object sender, EventArgs e)
	{
		string query = SearchEntry.Text;
		if (string.IsNullOrWhiteSpace(query))
		{
			await DisplayAlert("Error", "Please enter a search query", "OK");
			return;
		}

		try
		{
			await _viewModel.SearchAsync(query);
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
		}
	}
}
