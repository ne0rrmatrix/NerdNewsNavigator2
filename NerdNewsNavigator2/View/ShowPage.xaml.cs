using NerdNewsNavigator2.ViewModel;

namespace NerdNewsNavigator2.View;

public partial class ShowPage : ContentPage
{
	public ShowPage(ShowViewModel viewModel)
	{
		InitializeComponent();
		BindingContext= viewModel;
	}
}