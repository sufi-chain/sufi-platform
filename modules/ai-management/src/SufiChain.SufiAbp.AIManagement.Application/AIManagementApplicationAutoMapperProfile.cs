using Riok.Mapperly.Abstractions;
using SufiChain.SufiAbp.AIManagement.RAG;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiAbp.AIManagement;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WorkspaceToWorkspaceDtoMapper : MapperBase<Workspace, WorkspaceDto>
{
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasEmbedderConfig))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasVectorStoreConfig))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OpenAIApiMode))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.InputCostPer1KTokens))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OutputCostPer1KTokens))]
    public override partial WorkspaceDto Map(Workspace source);

    [MapperIgnoreTarget(nameof(WorkspaceDto.HasEmbedderConfig))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasVectorStoreConfig))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.HasApiKey))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OpenAIApiMode))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.InputCostPer1KTokens))]
    [MapperIgnoreTarget(nameof(WorkspaceDto.OutputCostPer1KTokens))]
    public override partial void Map(Workspace source, WorkspaceDto destination);

    public override void AfterMap(Workspace source, WorkspaceDto destination)
    {
        destination.HasApiKey = !string.IsNullOrEmpty(source.ApiKey);
        destination.HasEmbedderConfig = !string.IsNullOrEmpty(source.EmbedderConfigJson);
        destination.HasVectorStoreConfig = !string.IsNullOrEmpty(source.VectorStoreConfigJson);
        destination.OpenAIApiMode = source.OpenAIApiMode;
        destination.InputCostPer1KTokens = source.InputCostPer1KTokens;
        destination.OutputCostPer1KTokens = source.OutputCostPer1KTokens;
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
