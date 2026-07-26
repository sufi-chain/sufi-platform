using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.SufiAI.RAG;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.SufiAI;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WorkspaceToWorkspaceDtoMapper : MapperBase<Workspace, WorkspaceDto>
{
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OpenAIApiMode))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.InputCostPer1MTokens))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OutputCostPer1MTokens))]
    public override partial WorkspaceDto Map(Workspace source);

    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OpenAIApiMode))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.InputCostPer1MTokens))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OutputCostPer1MTokens))]
    public override partial void Map(Workspace source, WorkspaceDto destination);

    public override void AfterMap(Workspace source, WorkspaceDto destination)
    {
        destination.HasApiKey = !string.IsNullOrEmpty(source.ApiKey);
        destination.OpenAIApiMode = source.OpenAIApiMode;
        destination.InputCostPer1MTokens = source.InputCostPer1MTokens;
        destination.OutputCostPer1MTokens = source.OutputCostPer1MTokens;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class DocumentChunkToDocumentChunkDtoMapper : MapperBase<DocumentChunk, DocumentChunkDto>
{
    [MapperIgnoreSource(nameof(DocumentChunk.Embedding))]
    [MapperIgnoreSource(nameof(DocumentChunk.CreatedAt))]
    [MapperIgnoreSource(nameof(DocumentChunk.UpdatedAt))]
    [MapperIgnoreTarget(nameof(DocumentChunkDto.Score))]
    public override partial DocumentChunkDto Map(DocumentChunk source);

    [MapperIgnoreSource(nameof(DocumentChunk.Embedding))]
    [MapperIgnoreSource(nameof(DocumentChunk.CreatedAt))]
    [MapperIgnoreSource(nameof(DocumentChunk.UpdatedAt))]
    [MapperIgnoreTarget(nameof(DocumentChunkDto.Score))]
    public override partial void Map(DocumentChunk source, DocumentChunkDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IndexingStatusToIndexingStatusDtoMapper : MapperBase<IndexingStatus, IndexingStatusDto>
{
    public override partial IndexingStatusDto Map(IndexingStatus source);
    public override partial void Map(IndexingStatus source, IndexingStatusDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class RagAvailabilityToRagAvailabilityDtoMapper : MapperBase<RagAvailability, RagAvailabilityDto>
{
    public override partial RagAvailabilityDto Map(RagAvailability source);
    public override partial void Map(RagAvailability source, RagAvailabilityDto destination);
}
