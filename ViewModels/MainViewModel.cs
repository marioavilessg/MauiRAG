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

        // 🔥 SOLO lectura desde el servicio compartido
        public Workspace SelectedWorkspace
        {
            get => _ragService.CurrentWorkspace;
        }

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
                    Content = result.textResponse
                });
            }
            else
            {
                Response = result.textResponse;
            }

            Sources.Clear();

            if (result.sources != null)
            {
                foreach (var source in result.sources)
                    Sources.Add(source);
            }

            Question = string.Empty;
        }

        // 🔥 Método para refrescar cuando vuelves de Settings
        public void RefreshWorkspace()
        {
            OnPropertyChanged(nameof(SelectedWorkspace));
        }

        public async Task InitializeAsync()
        {
            if (_ragService.CurrentWorkspace != null)
                return;

            var list = await _ragService.GetWorkspacesAsync();

            if (list.Any())
            {
                var first = list.First();

                _ragService.CurrentWorkspace = first;
                _ragService.WorkspaceSlug = first.Slug;

                OnPropertyChanged(nameof(SelectedWorkspace));
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