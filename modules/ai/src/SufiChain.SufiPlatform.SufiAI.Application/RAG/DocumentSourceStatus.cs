namespace SufiChain.SufiPlatform.SufiAI.RAG;

public static class DocumentSourceStatus
{
    public static void Apply(DocumentSourceDto dto, string sourceName, int documentCount, IndexingStatus? status)
    {
        dto.Name = sourceName;
        dto.SourceName = sourceName;
        dto.DocumentCount = documentCount;
        if (status == null)
        {
            dto.Status = IndexingStatusType.Pending;
            dto.LastIndexedAt = null;
            dto.Progress = null;
            dto.ErrorMessage = null;
            return;
        }

        dto.LastIndexedAt = status.LastIndexedAt;
        dto.ErrorMessage = status.ErrorMessage;
        dto.Progress = status.TotalDocuments > 0
            ? status.IndexedDocuments * 100 / status.TotalDocuments
            : 0;
        dto.Status = status.IsIndexing
            ? IndexingStatusType.Indexing
            : !string.IsNullOrWhiteSpace(status.ErrorMessage)
                ? IndexingStatusType.Failed
                : status.TotalDocuments > 0 && status.IndexedDocuments >= status.TotalDocuments
                    ? IndexingStatusType.Complete
                    : IndexingStatusType.Pending;
    }
}
