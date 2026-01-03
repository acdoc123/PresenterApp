using PresenterApp.ViewModels;

namespace PresenterApp.Views;

public partial class PresentationBuilderPage : ContentPage
{
	public PresentationBuilderPage(PresentationBuilderViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}