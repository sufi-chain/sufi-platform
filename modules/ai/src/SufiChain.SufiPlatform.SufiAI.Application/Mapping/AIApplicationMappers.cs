using System.Linq;
using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Mapperly;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WorkspaceToDtoMapper : MapperBase<Workspace, WorkspaceDto>
{
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Guardrails))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Model))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.ModelDisplayName))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.DecisionsModelId))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.IsDefault))]
    public override partial WorkspaceDto Map(Workspace source);

    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Guardrails))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Model))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.ModelDisplayName))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.DecisionsModelId))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.IsDefault))]
    public override partial void Map(Workspace source, WorkspaceDto destination);

    public override void AfterMap(Workspace source, WorkspaceDto destination)
    {
        destination.HasApiKey = !string.IsNullOrWhiteSpace(source.ApiKey);
        destination.Model = source.Model;
        destination.ModelDisplayName = source.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)?.DisplayName;
        destination.DecisionsModelId = source.GetPrimaryConfiguration(AICapabilityType.Decisions)?.ModelId;
        destination.IsDefault = HostDefaultWorkspaceMarker.IsMarked(source);
        destination.Guardrails = source.Guardrails
            .Select(item => new WorkspaceGuardrailDto
            {
                Period = item.Period,
                AmountUsd = item.AmountUsd
            })
            .ToList();
    }
}
