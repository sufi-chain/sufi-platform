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

    public void SetProvider(AIProviderType provider)
    {
        Provider = provider;
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
        string? apiBaseUrl,
        string? displayName)
    {
        var configuration = GetPrimaryConfiguration(AICapabilityType.ChatCompletion);
        if (configuration == null)
        {
            AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                model,
                apiEndpoint: apiBaseUrl,
                displayName: displayName,
                isUserSelectable: true);
            return;
        }

        configuration.UpdateConfiguration(
            model,
            apiBaseUrl,
            configuration.ApiKey,
            configuration.Priority,
            OpenAIApiMode.ChatCompletions,
            configuration.InputPrice,
            configuration.OutputPrice,
            configuration.Dimensions,
            displayName,
            configuration.IsUserSelectable,
            configuration.Description,
            configuration.MaxContextTokens,
            configuration.InputPriceUnit,
            configuration.OutputPriceUnit);
    }

    public void SetDecisionsModel(string? modelId)
    {
        var existing = _modelConfigurations
            .Where(configuration => configuration.CapabilityType == AICapabilityType.Decisions)
            .ToList();
        if (string.IsNullOrWhiteSpace(modelId))
        {
            foreach (var configuration in existing)
            {
                configuration.Disable();
            }

            return;
        }

        var trimmed = modelId.Trim();
        var primary = existing.FirstOrDefault(configuration => configuration.IsEnabled)
            ?? existing.FirstOrDefault();
        if (primary == null)
        {
            AddModelConfiguration(AICapabilityType.Decisions, trimmed);
            return;
        }

        primary.Enable();
        primary.UpdateConfiguration(
            trimmed,
            primary.ApiEndpoint,
            primary.ApiKey,
            primary.Priority,
            OpenAIApiMode.ChatCompletions,
            primary.InputPrice,
            primary.OutputPrice,
            primary.Dimensions,
            primary.DisplayName,
            primary.IsUserSelectable,
            primary.Description,
            primary.MaxContextTokens,
            primary.InputPriceUnit,
            primary.OutputPriceUnit);
        foreach (var configuration in existing)
        {
            if (!ReferenceEquals(configuration, primary))
            {
                configuration.Disable();
            }
        }
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
        decimal? inputPrice = null,
        decimal? outputPrice = null,
        int? dimensions = null,
        string? displayName = null,
        bool isUserSelectable = false,
        string? description = null,
        int maxContextTokens = AIModelConfiguration.DefaultMaxContextTokens,
        AIPriceUnit? inputPriceUnit = null,
        AIPriceUnit? outputPriceUnit = null
    )
    {
        var config = new AIModelConfiguration(
            Guid.NewGuid(),
            Id,
            capabilityType,
            modelId,
            priority
        );
        var priceDefaults = CatalogPriceQuote.Default(capabilityType);

        config.UpdateConfiguration(
            modelId,
            apiEndpoint,
            apiKey,
            priority,
            openAIApiMode,
            inputPrice,
            outputPrice,
            dimensions,
            displayName,
            isUserSelectable,
            description,
            maxContextTokens,
            inputPriceUnit ?? priceDefaults.InputUnit,
            outputPriceUnit ?? priceDefaults.OutputUnit);
        
        _modelConfigurations.Add(config);
        return config;
    }

    /// <summary>
    /// Copies a route onto this workspace. A new id is used unless <paramref name="id"/> is set.
    /// Capability flags and <see cref="AIModelConfiguration.IsUserSelectable"/> are copied with the route.
    /// </summary>
    public AIModelConfiguration CopyModelConfiguration(AIModelConfiguration source, Guid? id = null)
    {
        var config = new AIModelConfiguration(
            id ?? Guid.NewGuid(),
            Id,
            source.CapabilityType,
            source.ModelId,
            source.Priority);
        config.UpdateConfiguration(
            source.ModelId,
            source.ApiEndpoint,
            source.ApiKey,
            source.Priority,
            source.OpenAIApiMode,
            source.InputPrice,
            source.OutputPrice,
            source.Dimensions,
            source.DisplayName,
            source.IsUserSelectable,
            source.Description,
            source.MaxContextTokens,
            source.InputPriceUnit,
            source.OutputPriceUnit);
        config.CopyChatCapabilitiesFrom(source);
        if (!source.IsEnabled)
        {
            config.Disable();
        }

        _modelConfigurations.Add(config);
        return config;
    }

    /// <summary>
    /// In-memory read model of this inherited projection. Model routes, connection, and guardrails
    /// come from the host source. Identity, name, and active state stay on the projection.
    /// The result is not a persistent entity.
    /// </summary>
    public Workspace CreateLiveInheritedView(Workspace source)
    {
        if (!IsInherited || SourceWorkspaceId == null || AssignmentId == null)
        {
            return this;
        }

        var view = new Workspace(
            Id,
            Name,
            source.Provider,
            string.IsNullOrWhiteSpace(source.DefaultModel) ? DefaultModel : source.DefaultModel,
            TenantId);
        view.UpdateConfiguration(
            string.IsNullOrWhiteSpace(source.DefaultModel) ? DefaultModel : source.DefaultModel,
            source.ApiKey,
            source.ApiBaseUrl,
            source.InputCostPer1MTokens,
            source.OutputCostPer1MTokens);
        if (!IsActive)
        {
            view.Deactivate();
        }

        view.MarkAsInherited(SourceWorkspaceId.Value, AssignmentId.Value);
        CopyAuditTo(view);

        foreach (var guardrail in source.Guardrails)
        {
            view.SetGuardrail(guardrail.Period, guardrail.AmountUsd);
        }

        foreach (var configuration in source.ModelConfigurations)
        {
            view.CopyModelConfiguration(configuration, configuration.Id);
        }

        return view;
    }

    /// <summary>
    /// Applies the few mutations a caller may make on a live inherited view
    /// back onto the persistent projection. Model routes stay on the stored row.
    /// </summary>
    public void CopyInheritedMutationFrom(Workspace view)
    {
        if (IsActive != view.IsActive)
        {
            if (view.IsActive)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }

        if (IsInherited && !view.IsInherited)
        {
            ClearInheritance();
        }

        if (IsDeleted != view.IsDeleted || DeleterId != view.DeleterId || DeletionTime != view.DeletionTime)
        {
            ObjectHelper.TrySetProperty(this, item => item.IsDeleted, () => view.IsDeleted);
            ObjectHelper.TrySetProperty(this, item => item.DeleterId, () => view.DeleterId);
            ObjectHelper.TrySetProperty(this, item => item.DeletionTime, () => view.DeletionTime);
        }
    }

    private void CopyAuditTo(Workspace view)
    {
        ObjectHelper.TrySetProperty(view, item => item.CreationTime, () => CreationTime);
        ObjectHelper.TrySetProperty(view, item => item.CreatorId, () => CreatorId);
        ObjectHelper.TrySetProperty(view, item => item.LastModificationTime, () => LastModificationTime);
        ObjectHelper.TrySetProperty(view, item => item.LastModifierId, () => LastModifierId);
        ObjectHelper.TrySetProperty(view, item => item.IsDeleted, () => IsDeleted);
        ObjectHelper.TrySetProperty(view, item => item.DeleterId, () => DeleterId);
        ObjectHelper.TrySetProperty(view, item => item.DeletionTime, () => DeletionTime);
        ObjectHelper.TrySetProperty(view, item => item.ConcurrencyStamp, () => ConcurrencyStamp);
    }

    /// <summary>
    /// Makes an enabled chat configuration the implicit workspace default.
    /// It becomes the lowest-priority enabled chat route, and <see cref="DefaultModel"/> is set to its model id.
    /// </summary>
    public void SetDefaultModelConfiguration(Guid configurationId)
    {
        var configuration = _modelConfigurations.FirstOrDefault(item => item.Id == configurationId);
        if (configuration == null)
        {
            throw new BusinessException(AIErrorCodes.ModelConfigurationNotFound)
                .WithData("ModelConfigurationId", configurationId);
        }

        if (configuration.CapabilityType != AICapabilityType.ChatCompletion)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceDefaultRequiresChatCompletion)
                .WithData("CapabilityType", configuration.CapabilityType.ToString());
        }

        if (!configuration.IsEnabled)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceDefaultRequiresEnabledConfiguration)
                .WithData("ModelConfigurationId", configurationId);
        }

        var peers = _modelConfigurations
            .Where(item =>
                item.Id != configuration.Id &&
                item.CapabilityType == AICapabilityType.ChatCompletion &&
                item.IsEnabled)
            .OrderBy(item => item.Priority)
            .ThenBy(item => item.Id)
            .ToList();

        if (peers.Count > 0 && configuration.Priority >= peers[0].Priority)
        {
            if (peers[0].Priority == int.MinValue)
            {
                configuration.SetPriority(0);
                var nextPriority = 1;
                foreach (var peer in peers)
                {
                    peer.SetPriority(nextPriority);
                    nextPriority++;
                }
            }
            else
            {
                configuration.SetPriority(peers[0].Priority - 1);
            }
        }

        DefaultModel = configuration.ModelId;
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
