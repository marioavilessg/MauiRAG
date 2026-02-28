using RagMaui.ViewModels;
using RagMaui.Views;

namespace RagMaui
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private readonly WorkspaceSettingsPage _settingsPage;

        public MainPage(
            MainViewModel viewModel,
            WorkspaceSettingsPage settingsPage)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _settingsPage = settingsPage;

            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.InitializeAsync();
            _viewModel.RefreshWorkspace();
        }

        private async void OnOpenSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(_settingsPage);
        }


    }
}