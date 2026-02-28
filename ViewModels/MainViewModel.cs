using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RagMaui.Services;
using RagMaui.Models;

namespace RagMaui.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RagService _ragService;

        public MainViewModel(RagService ragService)
        {
            _ragService = ragService;
        }

        private string _question;
        private string _response;
        private bool _isChatMode;

        #region Properties

        public string Question
        {
            get => _question;
            set { _question = value; OnPropertyChanged(); }
        }

        public string Response
        {
            get => _response;
            set { _response = value; OnPropertyChanged(); }
        }

        public bool IsChatMode
        {
            get => _isChatMode;
            set { _isChatMode = value; OnPropertyChanged(); }
        }

        public Workspace SelectedWorkspace
        {
            get => _ragService.CurrentWorkspace;
        }

        public string SelectedWorkspaceName =>
            _ragService.CurrentWorkspace?.Name ?? "Sin workspace";
        public ObservableCollection<Source> Sources { get; set; }
            = new ObservableCollection<Source>();

        public ObservableCollection<ChatMessage> Messages { get; set; }
            = new ObservableCollection<ChatMessage>();

        #endregion

        #region Commands

        public Command AskCommand => new(async () => await Ask());

        #endregion

        #region Methods

        private async Task Ask()
        {
            if (string.IsNullOrWhiteSpace(Question))
                return;

            if (_ragService.CurrentWorkspace == null)
            {
                Response = "No workspace seleccionado.";
                return;
            }

            var result = await _ragService.AskAsync(Question, IsChatMode);

            if (result == null)
            {
                Response = "Error en la respuesta.";
                return;
            }

            var answerText = result.GetText();

            if (string.IsNullOrWhiteSpace(answerText))
            {
                answerText = $"[INFO] No se encontró el texto. JSON Recibido:\n{result.RawJson}";
            }

            Response = answerText;

            if (IsChatMode)
            {
                Messages.Add(new ChatMessage
                {
                    Role = "user",
                    Content = Question
                });

                Messages.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = answerText
                });
            }

            Sources.Clear();

            if (result.sources != null)
            {
                foreach (var source in result.sources)
                    Sources.Add(source);
            }

            Question = string.Empty;
        }

        public void RefreshWorkspace()
        {
            OnPropertyChanged(nameof(SelectedWorkspace));
            OnPropertyChanged(nameof(SelectedWorkspaceName));
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_ragService.CurrentWorkspace != null)
                    return;

                var list = await _ragService.GetWorkspacesAsync();

                if (list != null && list.Any())
                {
                    var first = list.First();

                    _ragService.CurrentWorkspace = first;
                    _ragService.WorkspaceSlug = first.Slug;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnPropertyChanged(nameof(SelectedWorkspace));
                        OnPropertyChanged(nameof(SelectedWorkspaceName));
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MainViewModel Init: {ex.Message}");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}