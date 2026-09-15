using System.Linq;
using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.SufiAI.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WorkspaceToDtoMapper : MapperBase<Workspace, WorkspaceDto>
{
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Guardrails))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Model))]
    public override partial WorkspaceDto Map(Workspace source);

    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Guardrails))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.Model))]
    public override partial void Map(Workspace source, WorkspaceDto destination);

    public override void AfterMap(Workspace source, WorkspaceDto destination)
    {
        destination.HasApiKey = !string.IsNullOrWhiteSpace(source.ApiKey);
        destination.Model = source.Model;
        destination.Guardrails = source.Guardrails
            .Select(item => new WorkspaceGuardrailDto
            {
                Period = item.Period,
                AmountUsd = item.AmountUsd
            })
            .ToList();
    }
}
