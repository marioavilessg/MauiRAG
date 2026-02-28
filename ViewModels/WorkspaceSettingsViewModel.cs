using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RagMaui.Services;
using RagMaui.Models;

namespace RagMaui.ViewModels
{
    public class WorkspaceSettingsViewModel : INotifyPropertyChanged
    {
        private readonly RagService _ragService;

        public WorkspaceSettingsViewModel(RagService ragService)
        {
            _ragService = ragService;
        }

        private Workspace _selectedWorkspace;
        private string _newWorkspaceName;
        private string _systemPrompt;
        private string _renameWorkspaceName;

        #region Properties

        public ObservableCollection<Workspace> Workspaces { get; set; }
            = new ObservableCollection<Workspace>();

        public ObservableCollection<Document> Documents { get; set; }
            = new ObservableCollection<Document>();

        public Workspace SelectedWorkspace
        {
            get => _selectedWorkspace;
            set
            {
                _selectedWorkspace = value;
                OnPropertyChanged();

                if (value != null)
                {
                    _ragService.WorkspaceSlug = value.Slug;
                    _ragService.CurrentWorkspace = value;

                    _ = LoadDocumentsAsync();
                }
            }
        }

        public string NewWorkspaceName
        {
            get => _newWorkspaceName;
            set { _newWorkspaceName = value; OnPropertyChanged(); }
        }

        public string SystemPrompt
        {
            get => _systemPrompt;
            set { _systemPrompt = value; OnPropertyChanged(); }
        }

        public string RenameWorkspaceName
        {
            get => _renameWorkspaceName;
            set { _renameWorkspaceName = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public Command CreateWorkspaceCommand => new(async () => await CreateWorkspace());
        public Command DeleteWorkspaceCommand => new(async () => await DeleteWorkspace());
        public Command SavePromptCommand => new(async () => await SavePrompt());
        public Command RenameWorkspaceCommand => new(async () => await RenameWorkspace());

        public Command UploadDocumentCommand => new(async () => await UploadDocument());

        #endregion

        #region Methods

        public async Task LoadWorkspacesAsync()
        {
            try
            {
                var list = await _ragService.GetWorkspacesAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Workspaces.Clear();
                    if (list != null)
                    {
                        foreach (var ws in list)
                            Workspaces.Add(ws);
                    }

                    if (Workspaces.Any())
                    {
                        if (_ragService.CurrentWorkspace != null)
                        {
                            SelectedWorkspace = Workspaces
                                .FirstOrDefault(w => w.Slug == _ragService.CurrentWorkspace.Slug);
                        }
                        else
                        {
                            SelectedWorkspace = Workspaces.First();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadWorkspacesAsync: {ex.Message}");
            }
        }

        public async Task LoadDocumentsAsync()
        {
            try
            {
                if (SelectedWorkspace == null)
                    return;

                var docs = await _ragService.GetDocumentsAsync(SelectedWorkspace.Slug);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Documents.Clear();
                    if (docs != null)
                    {
                        foreach (var doc in docs)
                            Documents.Add(doc);
                    }
                });
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"Error LoadDocumentsAsync: {ex.Message}");
            }
        }

        private async Task UploadDocument()
        {
            if (SelectedWorkspace == null)
                return;

            var file = await FilePicker.PickAsync();

            if (file == null)
                return;

            bool success = await _ragService.UploadDocumentAsync(file.FullPath);

            if (success)
                await LoadDocumentsAsync();
        }

        private async Task CreateWorkspace()
        {
            if (string.IsNullOrWhiteSpace(NewWorkspaceName))
                return;

            var workspace = await _ragService.CreateWorkspaceAsync(NewWorkspaceName);

            if (workspace != null)
            {
                Workspaces.Add(workspace);
                SelectedWorkspace = workspace;
                NewWorkspaceName = string.Empty;
            }
        }

        private async Task DeleteWorkspace()
        {
            if (SelectedWorkspace == null)
                return;

            var slug = SelectedWorkspace.Slug;

            var success = await _ragService.DeleteWorkspaceAsync(slug);

            if (success)
            {
                Workspaces.Remove(SelectedWorkspace);

                if (Workspaces.Any())
                    SelectedWorkspace = Workspaces.First();
                else
                    SelectedWorkspace = null;
            }
        }

        private async Task SavePrompt()
        {
            if (SelectedWorkspace == null)
                return;

            await _ragService.UpdateSystemPromptAsync(
                SelectedWorkspace.Slug,
                SystemPrompt);
        }

        private async Task RenameWorkspace()
        {
            if (SelectedWorkspace == null || string.IsNullOrWhiteSpace(RenameWorkspaceName))
                return;

            bool success = await _ragService.RenameWorkspaceAsync(
                SelectedWorkspace.Slug,
                RenameWorkspaceName);

            if (success)
            {
                await LoadWorkspacesAsync();
                RenameWorkspaceName = string.Empty;
                OnPropertyChanged(nameof(RenameWorkspaceName));
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}