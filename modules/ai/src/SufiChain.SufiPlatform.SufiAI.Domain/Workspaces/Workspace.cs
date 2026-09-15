using System;
using System.Collections.Generic;
using System.Linq;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class Workspace : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    /// <summary>Host-owned workspace projection metadata.</summary>
    public bool IsInherited { get; protected set; }
    public Guid? SourceWorkspaceId { get; protected set; }
    public Guid? AssignmentId { get; protected set; }
    
    public string Name { get; protected set; } = string.Empty;
    public AIProviderType Provider { get; protected set; }
    
    /// <summary>
    /// Default/fallback model ID for this workspace. Used when no specific model configuration exists for a capability.
    /// </summary>
    public string DefaultModel { get; protected set; } = string.Empty;
    
    public string? ApiKey { get; protected set; }
    public string? ApiBaseUrl { get; protected set; }
    public decimal? InputCostPer1MTokens { get; protected set; }
    public decimal? OutputCostPer1MTokens { get; protected set; }
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Collection of AI model configurations for different capabilities.
    /// Replaces the single Model property to support multi-modal AI.
    /// </summary>
    private readonly List<AIModelConfiguration> _modelConfigurations = new();
    public IReadOnlyList<AIModelConfiguration> ModelConfigurations => _modelConfigurations.AsReadOnly();

    private readonly List<WorkspaceGuardrail> _guardrails = new();
    public IReadOnlyList<WorkspaceGuardrail> Guardrails => _guardrails.AsReadOnly();
    
    /// <summary>
    /// Gets the model ID - returns primary chat completion model or the workspace's default model
    /// </summary>
    public string Model => GetPrimaryConfiguration(AICapabilityType.ChatCompletion)?.ModelId ?? DefaultModel;
    
    protected Workspace() { }
    
    public Workspace(
        Guid id,
        string name,
        AIProviderType provider,
        string model,
        Guid? tenantId = null
    ) : base(id)
    {
        SetName(name);
        Provider = provider;
        DefaultModel = Check.NotNullOrWhiteSpace(model, nameof(model));
        TenantId = tenantId;
        IsActive = true;
    }
    
    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
    }
    
    public void UpdateConfiguration(
        string model,
        string? apiKey,
        string? apiBaseUrl,
        decimal? inputCostPer1MTokens = null,
        decimal? outputCostPer1MTokens = null
    )
    {
        ValidatePricing(inputCostPer1MTokens, nameof(inputCostPer1MTokens));
        ValidatePricing(outputCostPer1MTokens, nameof(outputCostPer1MTokens));

        DefaultModel = Check.NotNullOrWhiteSpace(model, nameof(model));
        ApiKey = apiKey;
        ApiBaseUrl = apiBaseUrl;
        InputCostPer1MTokens = inputCostPer1MTokens;
        OutputCostPer1MTokens = outputCostPer1MTokens;
    }

    public void UpdatePrimaryChatConfiguration(
        string model,
        string? apiBaseUrl)
    {
        var configuration = GetPrimaryConfiguration(AICapabilityType.ChatCompletion);
        if (configuration == null)
        {
            return;
        }

        configuration.UpdateConfiguration(
            model,
            apiBaseUrl,
            configuration.ApiKey,
            configuration.Priority,
            configuration.OpenAIApiMode,
            configuration.InputCostPer1MTokens,
            configuration.OutputCostPer1MTokens,
            configuration.Dimensions,
            configuration.DisplayName,
            configuration.IsUserSelectable,
            configuration.Description,
            configuration.MaxContextTokens);
    }
    
    /// <summary>
    /// Add a model configuration for a specific capability
    /// </summary>
    public AIModelConfiguration AddModelConfiguration(
        AICapabilityType capabilityType,
        string modelId,
        string? apiEndpoint = null,
        string? apiKey = null,
        int priority = 0,
        OpenAIApiMode openAIApiMode = OpenAIApiMode.ChatCompletions,
        decimal? inputCostPer1MTokens = null,
        decimal? outputCostPer1MTokens = null,
        int? dimensions = null,
        string? displayName = null,
        bool isUserSelectable = false,
        string? description = null,
        int maxContextTokens = AIModelConfiguration.DefaultMaxContextTokens
    )
    {
        var config = new AIModelConfiguration(
            Guid.NewGuid(),
            Id,
            capabilityType,
            modelId,
            priority
        );
        
        config.UpdateConfiguration(
            modelId,
            apiEndpoint,
            apiKey,
            priority,
            openAIApiMode,
            inputCostPer1MTokens,
            outputCostPer1MTokens,
            dimensions,
            displayName,
            isUserSelectable,
            description,
            maxContextTokens);
        
        _modelConfigurations.Add(config);
        return config;
    }

    /// <summary>
    /// Get the primary (highest priority) configuration for a capability
    /// </summary>
    public AIModelConfiguration? GetPrimaryConfiguration(AICapabilityType capabilityType)
    {
        return _modelConfigurations
            .Where(c => c.CapabilityType == capabilityType && c.IsEnabled)
            .OrderBy(c => c.Priority)
            .FirstOrDefault();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void MarkAsInherited(Guid sourceWorkspaceId, Guid assignmentId)
    {
        if (TenantId == null)
        {
            throw new BusinessException(AIErrorCodes.InheritedWorkspaceMustBelongToTenant);
        }

        IsInherited = true;
        SourceWorkspaceId = sourceWorkspaceId;
        AssignmentId = assignmentId;
    }

    public void ClearInheritance()
    {
        IsInherited = false;
        SourceWorkspaceId = null;
        AssignmentId = null;
    }

    public WorkspaceGuardrail SetGuardrail(WorkspaceGuardrailPeriod period, decimal amountUsd)
    {
        var guardrail = _guardrails.FirstOrDefault(x => x.Period == period);
        if (guardrail == null)
        {
            guardrail = new WorkspaceGuardrail(Guid.NewGuid(), Id, period, amountUsd);
            _guardrails.Add(guardrail);
        }
        else
        {
            guardrail.Set(period, amountUsd);
        }
        return guardrail;
    }

    public void RemoveGuardrail(WorkspaceGuardrailPeriod period)
    {
        var guardrail = _guardrails.FirstOrDefault(x => x.Period == period);
        if (guardrail != null) _guardrails.Remove(guardrail);
    }

    private static void ValidatePricing(decimal? value, string parameterName)
    {
        if (value < 0)
        {
            throw new BusinessException("AI:InvalidTokenPricing")
                .WithData("ParameterName", parameterName);
        }
    }
}
