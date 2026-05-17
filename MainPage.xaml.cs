namespace SplitMoney.Client;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		rootComponent.ComponentType = typeof(Components.Routes);
	}
}
