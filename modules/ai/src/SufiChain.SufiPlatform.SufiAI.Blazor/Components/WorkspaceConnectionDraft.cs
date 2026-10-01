using System.Globalization;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public sealed class WorkspaceConnectionDraft
{
    public string Name { get; set; } = string.Empty;

    public AIProviderType Provider { get; set; } = AIProviderType.OpenAI;

    public string Model { get; set; } = string.Empty;

    public string? ModelDisplayName { get; set; }

    public string? DecisionsModelId { get; set; }

    public string? ApiKey { get; set; }

    public string? ApiBaseUrl { get; set; }

    public string InputCostText { get; set; } = string.Empty;

    public string OutputCostText { get; set; } = string.Empty;

    public static WorkspaceConnectionDraft ForCreate() => new();

    public static WorkspaceConnectionDraft From(WorkspaceDto workspace)
    {
        return new WorkspaceConnectionDraft
        {
            Name = workspace.Name,
            Provider = workspace.Provider,
            Model = workspace.Model,
            ModelDisplayName = workspace.ModelDisplayName,
            DecisionsModelId = workspace.DecisionsModelId,
            ApiBaseUrl = workspace.ApiBaseUrl,
            InputCostText = workspace.InputCostPer1MTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            OutputCostText = workspace.OutputCostPer1MTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    public void ApplyProvider(AiProviderProfileDto profile)
    {
        Provider = profile.ProviderType;
        ApiBaseUrl = profile.RequiresExplicitBaseUrl ? string.Empty : profile.DefaultBaseUrl ?? string.Empty;
        Model = string.Empty;
        ModelDisplayName = null;
        DecisionsModelId = null;
        InputCostText = string.Empty;
        OutputCostText = string.Empty;
    }

    public void ApplyChatModel(OpenAIModelDto model)
    {
        Model = model.Id;
        ModelDisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? model.Id : model.DisplayName;
        if (model.SuggestedInputCostPer1MTokens is decimal input)
        {
            InputCostText = input.ToString("0.####", CultureInfo.InvariantCulture);
        }

        if (model.SuggestedOutputCostPer1MTokens is decimal output)
        {
            OutputCostText = output.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }

    public CreateWorkspaceDto ToCreate(decimal? inputCost, decimal? outputCost)
    {
        return new CreateWorkspaceDto
        {
            Name = Name,
            Provider = Provider,
            Model = Model,
            ModelDisplayName = BlankToNull(ModelDisplayName),
            DecisionsModelId = BlankToNull(DecisionsModelId),
            ApiKey = ApiKey,
            ApiBaseUrl = ApiBaseUrl,
            InputCostPer1MTokens = inputCost,
            OutputCostPer1MTokens = outputCost
        };
    }

    public UpdateWorkspaceDto ToUpdate(decimal? inputCost, decimal? outputCost, bool isActive)
    {
        return new UpdateWorkspaceDto
        {
            Name = Name,
            Provider = Provider,
            Model = Model,
            ModelDisplayName = BlankToNull(ModelDisplayName),
            DecisionsModelId = BlankToNull(DecisionsModelId),
            ApiKey = ApiKey,
            ApiBaseUrl = ApiBaseUrl,
            InputCostPer1MTokens = inputCost,
            OutputCostPer1MTokens = outputCost,
            IsActive = isActive
        };
    }

    public static List<OpenAIModelDto> WithCurrent(
        IEnumerable<OpenAIModelDto> models,
        string? id,
        string? displayName)
    {
        var list = models.ToList();
        if (!string.IsNullOrWhiteSpace(id) &&
            !list.Any(model => string.Equals(model.Id, id, StringComparison.OrdinalIgnoreCase)))
        {
            list.Insert(0, new OpenAIModelDto
            {
                Id = id,
                DisplayName = displayName
            });
        }

        return list;
    }

    public static string SearchText(OpenAIModelDto model)
    {
        return string.Join(' ', new[] { model.DisplayName, model.Id }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string? BlankToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
