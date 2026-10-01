using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Blazor;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceConnectionFields : AIComponentBase
{
    [Parameter, EditorRequired]
    public WorkspaceConnectionDraft Draft { get; set; } = new();

    [Parameter]
    public IReadOnlyList<AiProviderProfileDto> Profiles { get; set; } = Array.Empty<AiProviderProfileDto>();

    [Parameter]
    public IReadOnlyList<OpenAIModelDto> AvailableModels { get; set; } = Array.Empty<OpenAIModelDto>();

    [Parameter]
    public IReadOnlyList<OpenAIModelDto> DecisionModels { get; set; } = Array.Empty<OpenAIModelDto>();

    [Parameter]
    public bool ModelsLoaded { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public bool ShowSavedKeyHint { get; set; }

    [Parameter]
    public bool LoadingModels { get; set; }

    [Parameter]
    public EventCallback LoadModels { get; set; }

    [Parameter]
    public EventCallback ProviderReset { get; set; }

    private AiProviderProfileDto? SelectedProfile =>
        Profiles.FirstOrDefault(profile => profile.ProviderType == Draft.Provider);

    private bool RequiresBaseUrl => SelectedProfile?.RequiresExplicitBaseUrl == true;

    private string? ChatValue => string.IsNullOrWhiteSpace(Draft.Model) ? null : Draft.Model;

    private IReadOnlyList<OpenAIModelDto> ChatModels =>
        WorkspaceConnectionDraft.WithCurrent(AvailableModels, Draft.Model, Draft.ModelDisplayName);

    private IReadOnlyList<OpenAIModelDto> DecisionChoices =>
        WorkspaceConnectionDraft.WithCurrent(DecisionModels, Draft.DecisionsModelId, displayName: null);

    private string EndpointHint
    {
        get
        {
            if (RequiresBaseUrl && string.IsNullOrWhiteSpace(Draft.ApiBaseUrl))
            {
                return L["ProviderEndpointRequired"].Value;
            }

            if (SelectedProfile?.DefaultBaseUrl is { Length: > 0 } defaultUrl &&
                (string.IsNullOrWhiteSpace(Draft.ApiBaseUrl) ||
                 string.Equals(Draft.ApiBaseUrl.Trim(), defaultUrl, StringComparison.OrdinalIgnoreCase)))
            {
                return L["ProviderEndpointDefault"].Value;
            }

            return L["ProviderEndpointCustom"].Value;
        }
    }

    private string ProviderHint(AiProviderProfileDto profile) =>
        string.IsNullOrWhiteSpace(profile.DefaultBaseUrl)
            ? L["ProviderEndpointRequired"].Value
            : profile.DefaultBaseUrl;

    private Task InvokeLoadModels() => LoadModels.InvokeAsync();

    private async Task OnProviderChangedAsync(AIProviderType provider)
    {
        if (ReadOnly)
        {
            return;
        }

        var profile = Profiles.FirstOrDefault(item => item.ProviderType == provider)
            ?? new AiProviderProfileDto
            {
                ProviderType = provider,
                RequiresExplicitBaseUrl = true
            };
        Draft.ApplyProvider(profile);
        await ProviderReset.InvokeAsync();
    }

    private Task OnChatModelChangedAsync(string? modelId)
    {
        if (ReadOnly || string.IsNullOrWhiteSpace(modelId))
        {
            return Task.CompletedTask;
        }

        var model = ChatModels.FirstOrDefault(item => string.Equals(item.Id, modelId, StringComparison.OrdinalIgnoreCase));
        if (model != null)
        {
            Draft.ApplyChatModel(model);
        }

        return Task.CompletedTask;
    }

    private Task OnDecisionsChangedAsync(string? modelId)
    {
        if (!ReadOnly)
        {
            Draft.DecisionsModelId = string.IsNullOrWhiteSpace(modelId) ? null : modelId;
        }

        return Task.CompletedTask;
    }
}
