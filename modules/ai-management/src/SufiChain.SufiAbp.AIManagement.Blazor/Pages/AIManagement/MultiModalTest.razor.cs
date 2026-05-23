using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Permissions;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class MultiModalTest : AIManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string SendChat = "send-chat";
        public const string TranscribeAudio = "transcribe-audio";
        public const string GenerateSpeech = "generate-speech";
        public const string AnalyzeImage = "analyze-image";
        public const string GenerateEmbeddings = "generate-embeddings";
    }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private List<WorkspaceDto> _workspaces = new();
    private Guid? _selectedWorkspaceId;
    private string _selectedWorkspaceName = string.Empty;
    private AICapabilityType _selectedCapability = AICapabilityType.ChatCompletion;
    private string _fileManagerBaseUrl = "/admin/file-manager/files";

    // Chat
    private string _chatMessage = string.Empty;
    private string _chatResponse = string.Empty;
    private int? _chatTokens;
    private string _chatModel = string.Empty;

    // Audio Transcription
    private byte[]? _audioData;
    private string? _audioFileName;
    private string _transcriptionText = string.Empty;
    private Guid? _transcriptionFileId;
    private string? _transcriptionFileUrl;
    private string _transcriptionModel = string.Empty;

    // Text-to-Speech
    private string _ttsText = string.Empty;
    private byte[]? _audioOutputData;
    private string? _audioOutputUrl;
    private Guid? _ttsFileId;
    private string? _ttsFileUrl;

    // Vision
    private byte[]? _imageData;
    private string? _imageFileName;
    private string? _imagePreviewUrl;
    private Guid? _visionFileId;
    private string? _visionFileUrl;
    private string _visionPrompt = string.Empty;
    private string _visionDescription = string.Empty;
    private int _visionTokens;
    private string _visionModel = string.Empty;

    // Embeddings
    private string _embeddingsText = string.Empty;
    private float[]? _embeddingsVector;
    private string _embeddingsModel = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadWorkspacesAsync();
    }

    private async Task LoadWorkspacesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await WorkspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000
            });
            _workspaces = result.Items.ToList();
        }, LoadingKeys.LoadWorkspaces);
    }

    private Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        
        var workspace = _workspaces.FirstOrDefault(w => w.Id == workspaceId);
        _selectedWorkspaceName = workspace?.Name ?? string.Empty;
        
        ResetForm();
        return Task.CompletedTask;
    }

    private void SelectCapability(AICapabilityType capability)
    {
        _selectedCapability = capability;
    }

    private void ResetForm()
    {
        _chatMessage = string.Empty;
        _chatResponse = string.Empty;
        _chatTokens = 0;
        _chatModel = string.Empty;

        _audioData = null;
        _audioFileName = null;
        _transcriptionText = string.Empty;
        _transcriptionFileId = null;
        _transcriptionFileUrl = null;
        _transcriptionModel = string.Empty;

        _ttsText = string.Empty;
        _audioOutputData = null;
        _audioOutputUrl = null;
        _ttsFileId = null;
        _ttsFileUrl = null;

        _imageData = null;
        _imageFileName = null;
        _imagePreviewUrl = null;
        _visionFileId = null;
        _visionFileUrl = null;
        _visionPrompt = string.Empty;
        _visionDescription = string.Empty;
        _visionTokens = 0;
        _visionModel = string.Empty;

        _embeddingsText = string.Empty;
        _embeddingsVector = null;
        _embeddingsModel = string.Empty;
    }

    // Chat
    private async Task SendChatAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var input = new SendChatMessageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Message = _chatMessage
            };

            var response = await AIAppService.SendChatMessageAsync(input);
            _chatResponse = response.Message;
            _chatTokens = response.TokensUsed;
            _chatModel = response.Model;
        }, LoadingKeys.SendChat);
    }

    // Audio Transcription
    private async Task OnAudioFileSelectedAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            _audioFileName = file.Name;
            using var stream = file.OpenReadStream(maxAllowedSize: 25 * 1024 * 1024); // 25MB
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            _audioData = ms.ToArray();
        }
    }

    private async Task TranscribeAudioAsync()
    {
        if (_audioData == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var input = new TranscribeAudioInput
            {
                WorkspaceName = _selectedWorkspaceName,
                AudioData = _audioData,
                AudioFormat = Path.GetExtension(_audioFileName)?.TrimStart('.') ?? "mp3"
            };

            var response = await AIAppService.TranscribeAudioAsync(input);
            _transcriptionText = response.Text;
            _transcriptionFileId = response.FileId;
            _transcriptionFileUrl = response.FileUrl;
            _transcriptionModel = response.Model;
        }, LoadingKeys.TranscribeAudio);
    }

    // Text-to-Speech
    private async Task GenerateSpeechAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var input = new GenerateSpeechInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Text = _ttsText
            };

            var response = await AIAppService.GenerateSpeechAsync(input);
            _audioOutputData = response.AudioData;
            _ttsFileId = response.FileId;
            _ttsFileUrl = response.FileUrl;
            
            // Create blob URL for audio playback
            var base64 = Convert.ToBase64String(_audioOutputData);
            _audioOutputUrl = $"data:audio/{response.AudioFormat};base64,{base64}";
        }, LoadingKeys.GenerateSpeech);
    }

    private async Task DownloadAudioAsync()
    {
        if (_audioOutputData == null) return;

        var fileName = $"speech_{DateTime.Now:yyyyMMddHHmmss}.mp3";
        await JSRuntime.InvokeVoidAsync("downloadFile", fileName, Convert.ToBase64String(_audioOutputData), "audio/mpeg");
    }

    // Vision Analysis
    private async Task OnImageFileSelectedAsync(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            _imageFileName = file.Name;
            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            _imageData = ms.ToArray();

            // Create preview URL
            var base64 = Convert.ToBase64String(_imageData);
            var extension = Path.GetExtension(_imageFileName)?.TrimStart('.').ToLower() ?? "png";
            _imagePreviewUrl = $"data:image/{extension};base64,{base64}";
        }
    }

    private async Task AnalyzeImageAsync()
    {
        if (_imageData == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var input = new AnalyzeImageInput
            {
                WorkspaceName = _selectedWorkspaceName,
                ImageData = _imageData,
                ImageFormat = Path.GetExtension(_imageFileName)?.TrimStart('.') ?? "png",
                Prompt = _visionPrompt
            };

            var response = await AIAppService.AnalyzeImageAsync(input);
            _visionDescription = response.Description;
            _visionFileId = response.FileId;
            _visionFileUrl = response.FileUrl;
            _visionTokens = (response.InputTokens ?? 0) + (response.OutputTokens ?? 0);
            _visionModel = response.Model;
        }, LoadingKeys.AnalyzeImage);
    }

    // Embeddings
    private async Task GenerateEmbeddingsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var input = new GenerateEmbeddingsInput
            {
                WorkspaceName = _selectedWorkspaceName,
                Text = _embeddingsText
            };

            var response = await AIAppService.GenerateEmbeddingsAsync(input);
            _embeddingsVector = response.Embedding;
            _embeddingsModel = response.Model;
        }, LoadingKeys.GenerateEmbeddings);
    }
}
