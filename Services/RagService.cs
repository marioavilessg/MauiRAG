using System.Text;
using System.Text.Json;
using RagMaui.Models;

namespace RagMaui.Services
{
    public class RagService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "http://localhost:3001";
        private const string ApiKey = "YMY9AJV-HAV4A4Y-KP6FAZT-K1DETTV";

        public string WorkspaceSlug { get; set; }

        // 🔥 Workspace activo compartido en toda la app
        public Workspace CurrentWorkspace { get; set; }

        private int? _chatId = null;

        public RagService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
        }

        #region Ask (Chat / Query)

        public async Task<RagResponse> AskAsync(string message, bool isChatMode)
        {
            if (string.IsNullOrEmpty(WorkspaceSlug))
                throw new Exception("No workspace selected.");

            var body = new
            {
                message = message,
                mode = isChatMode ? "chat" : "query"
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (isChatMode && _chatId.HasValue)
            {
                response = await _httpClient.PostAsync(
                    $"{BaseUrl}/api/v1/workspace/{WorkspaceSlug}/chat/{_chatId}",
                    content);
            }
            else
            {
                response = await _httpClient.PostAsync(
                    $"{BaseUrl}/api/v1/workspace/{WorkspaceSlug}/chat",
                    content);
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<RagResponse>(
                responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (isChatMode && result != null)
                _chatId = result.chatId;

            if (!isChatMode)
                _chatId = null;

            return result;
        }

        public void ResetChat()
        {
            _chatId = null;
        }

        #endregion

        #region Workspace Config

        public async Task<bool> UpdateSystemPromptAsync(string slug, string newPrompt)
        {
            var body = new
            {
                openAiPrompt = newPrompt
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/api/v1/workspace/{slug}/update",
                content);

            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Workspaces

        public async Task<List<Workspace>> GetWorkspacesAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/v1/workspaces");

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<WorkspaceListResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Workspaces ?? new List<Workspace>();
        }

        public async Task<Workspace> CreateWorkspaceAsync(string name)
        {
            var body = new { name = name };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/api/v1/workspace/new",
                content);

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<CreateWorkspaceResponse>(
                responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Workspace;
        }

        public async Task<bool> DeleteWorkspaceAsync(string slug)
        {
            var response = await _httpClient.DeleteAsync(
                $"{BaseUrl}/api/v1/workspace/{slug}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RenameWorkspaceAsync(string slug, string newName)
        {
            var body = new { name = newName };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/api/v1/workspace/{slug}/update",
                content);

            return response.IsSuccessStatusCode;
        }

        #endregion
    }
}