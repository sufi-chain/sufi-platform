using System;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Indexing status of a document source within a workspace.
/// </summary>
public class SufiAbpAIIndexingStatus
{
    /// <summary>
    /// Document source name.
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of indexable documents in the source.
    /// </summary>
    public int TotalDocuments { get; set; }

    /// <summary>
    /// Number of documents currently present in the index.
    /// </summary>
    public int IndexedDocuments { get; set; }

    /// <summary>
    /// Timestamp of the last completed indexing run, when known.
    /// </summary>
    public DateTime? LastIndexedAt { get; set; }

    /// <summary>
    /// Whether an indexing run is currently in progress.
    /// </summary>
    public bool IsIndexing { get; set; }
}
