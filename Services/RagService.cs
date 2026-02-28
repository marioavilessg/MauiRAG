using RagMaui.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DocumentModel = RagMaui.Models.Document;

namespace RagMaui.Services
{
    public class RagService
    {
        private readonly HttpClient _httpClient;

        private const string BaseUrl = "http://localhost:3001/api/v1";
        private const string ApiKey = "YMY9AJV-HAV4A4Y-KP6FAZT-K1DETTV";

        public string WorkspaceSlug { get; set; }
        public Workspace CurrentWorkspace { get; set; }

        public RagService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
        }

        #region Ask (SIN memoria acumulada para evitar overflow)

        public async Task<RagResponse> AskAsync(string message, bool isChatMode)
        {
            if (string.IsNullOrEmpty(WorkspaceSlug))
                throw new Exception("No workspace selected.");

            var body = new
            {
                message = message,
                mode = "chat"
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/workspace/{WorkspaceSlug}/chat",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<RagResponse>(
                responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null)
                result.RawJson = responseJson;

            return result;
        }

        #endregion

        #region Workspace Config

        public async Task<bool> UpdateSystemPromptAsync(string slug, string newPrompt)
        {
            var body = new { openAiPrompt = newPrompt };

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/workspace/{slug}/update",
                body);

            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Workspaces

        public async Task<List<Workspace>> GetWorkspacesAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/workspaces");

            if (!response.IsSuccessStatusCode)
                return new List<Workspace>();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<WorkspaceListResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Workspaces ?? new List<Workspace>();
        }

        public async Task<Workspace> CreateWorkspaceAsync(string name)
        {
            var body = new { name = name };

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/workspace/new",
                body);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<CreateWorkspaceResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Workspace;
        }

        public async Task<bool> DeleteWorkspaceAsync(string slug)
        {
            var response = await _httpClient.DeleteAsync(
                $"{BaseUrl}/workspace/{slug}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RenameWorkspaceAsync(string slug, string newName)
        {
            var body = new { name = newName };

            var response = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/workspace/{slug}/update",
                body);

            return response.IsSuccessStatusCode;
        }

        #endregion

        #region Documents

        public async Task<bool> UploadDocumentAsync(string filePath)
        {
            if (string.IsNullOrEmpty(WorkspaceSlug))
                return false;

            var form = new MultipartFormDataContent();

            var fileBytes = File.ReadAllBytes(filePath);
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var uploadResponse = await _httpClient.PostAsync(
                $"{BaseUrl}/document/upload",
                form);

            if (!uploadResponse.IsSuccessStatusCode)
                return false;

            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();

            using var docJson = JsonDocument.Parse(uploadJson);

            var fullPath = docJson.RootElement
                .GetProperty("documents")[0]
                .GetProperty("location")
                .GetString();

            if (string.IsNullOrEmpty(fullPath))
                return false;

            var location = fullPath.Substring(
                fullPath.IndexOf("custom-documents"));

            location = location.Replace("\\", "/");

            var payload = new
            {
                adds = new[] { location },
                deletes = Array.Empty<string>()
            };

            var updateResponse = await _httpClient.PostAsJsonAsync(
                $"{BaseUrl}/workspace/{WorkspaceSlug}/update-embeddings",
                payload);

            return updateResponse.IsSuccessStatusCode;
        }

        public async Task<bool> ReindexWorkspaceAsync(string slug)
        {
            var response = await _httpClient.PostAsync(
                $"{BaseUrl}/workspace/{slug}/reindex",
                null);

            return response.IsSuccessStatusCode;
        }

        public async Task<List<DocumentModel>> GetDocumentsAsync(string slug)
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/workspace/{slug}");

            if (!response.IsSuccessStatusCode)
                return new List<DocumentModel>();

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<WorkspaceSingleResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Workspace == null || !result.Workspace.Any())
                return new List<DocumentModel>();

            return result.Workspace.First().Documents ?? new List<DocumentModel>();
        }

        #endregion
    }
}