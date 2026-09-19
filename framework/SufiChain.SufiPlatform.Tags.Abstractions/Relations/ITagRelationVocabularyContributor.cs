using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>Declarative owning-module vocabulary; Tags owns persistence and tenant seeding.</summary>
public interface ITagRelationVocabularyContributor
{
    string Scope { get; }
    IReadOnlyList<TagRelationVocabularyEntry> GetEntries();
}

public sealed class TagRelationVocabularyEntry
{
    public string StableKey { get; }
    public string SourceType { get; }
    public string TargetType { get; }
    public bool IsSymmetric { get; }
    public bool RequiresAcyclicGraph { get; }

    public TagRelationVocabularyEntry(string stableKey, string sourceType, string targetType,
        bool isSymmetric = false, bool requiresAcyclicGraph = false)
    {
        if (!TagEntityReference.IsValidEntityType(stableKey) || stableKey.Length > 64 ||
            !TagEntityReference.IsValidEntityType(sourceType) || !TagEntityReference.IsValidEntityType(targetType))
            throw new ArgumentException("Canonical vocabulary keys and endpoint types are required.");
        if ((isSymmetric && sourceType != targetType) ||
            (requiresAcyclicGraph && (isSymmetric || sourceType != targetType)))
            throw new ArgumentException("Invalid symmetric or acyclic vocabulary definition.");
        StableKey = stableKey;
        SourceType = sourceType;
        TargetType = targetType;
        IsSymmetric = isSymmetric;
        RequiresAcyclicGraph = requiresAcyclicGraph;
    }
}
