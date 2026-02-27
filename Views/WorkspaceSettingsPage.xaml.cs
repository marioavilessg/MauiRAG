using RagMaui.ViewModels;

namespace RagMaui.Views;

public partial class WorkspaceSettingsPage : ContentPage
{
    private readonly WorkspaceSettingsViewModel _viewModel;

    public WorkspaceSettingsPage(WorkspaceSettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadWorkspacesAsync();
    }
}