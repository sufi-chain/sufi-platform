using Qdrant.Client.Grpc;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Qdrant;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.RAG;

/// <summary>
/// Regression for the live defect where points carried a flat key literally named "meta.projectId"
/// while the filter addressed the nested path meta -> projectId, so every projectId filter matched 0 points.
/// The payload and the filter must agree on the nested representation.
/// </summary>
public class QdrantPayloadMetadataTests
{
    private static readonly Guid ProjectId = Guid.Parse("00645343-64fd-0040-5478-3a23944a5697");
    private static readonly Guid ArticleId = Guid.Parse("bc852242-5cec-0b1f-0a00-3a2394c01339");

    [Fact]
    public void Payload_Should_Store_Metadata_As_Nested_Meta_Object()
    {
        var payload = QdrantPayloadMetadata.BuildPayloadValue(new Dictionary<string, object>
        {
            ["projectId"] = ProjectId,
            ["articleId"] = ArticleId.ToString("D"),
            ["empty"] = "   ",
            ["nullValue"] = null!
        });

        payload.KindCase.ShouldBe(Value.KindOneofCase.StructValue);
        payload.StructValue.Fields.Keys.ShouldBe(new[] { "projectId", "articleId" }, ignoreOrder: true);
        payload.StructValue.Fields["projectId"].StringValue.ShouldBe(ProjectId.ToString("D"));
        payload.StructValue.Fields["articleId"].StringValue.ShouldBe(ArticleId.ToString("D"));
        payload.StructValue.Fields.Keys.ShouldNotContain(key => key.Contains('.'));
    }

    [Fact]
    public void ProjectId_Filter_Path_Should_Resolve_Against_Stored_Payload()
    {
        var pointPayload = new Dictionary<string, Value>
        {
            [QdrantPayloadMetadata.RootKey] = QdrantPayloadMetadata.BuildPayloadValue(new Dictionary<string, object>
            {
                ["projectId"] = ProjectId,
                ["articleId"] = ArticleId
            })
        };

        var filter = QdrantVectorStoreProvider.BuildWorkspaceFilter(new VectorStoreContext
        {
            TenantKey = "host",
            WorkspaceName = "default",
            SourceName = "KnowledgeBase",
            MetadataFilters = new Dictionary<string, string> { ["projectId"] = ProjectId.ToString("D") }
        });

        var projectCondition = filter.Must
            .Select(condition => condition.Field)
            .Single(field => field != null && field.Key == QdrantPayloadMetadata.ToFilterKey("projectId"));

        projectCondition.Key.ShouldBe("meta.projectId");
        ResolvePath(pointPayload, projectCondition.Key).ShouldBe(projectCondition.Match.Keyword);
    }

    [Fact]
    public void Flat_Dotted_Key_Should_Not_Be_Reachable_By_Nested_Filter_Path()
    {
        // The legacy representation that produced "meta.projectId on 0 points" in the live collection.
        var legacyPayload = new Dictionary<string, Value>
        {
            ["meta.projectId"] = new Value { StringValue = ProjectId.ToString("D") }
        };

        ResolvePath(legacyPayload, QdrantPayloadMetadata.ToFilterKey("projectId")).ShouldBeNull();
    }

    [Fact]
    public void Filter_Should_Scope_By_Tenant_Workspace_And_Source_Before_Metadata()
    {
        var filter = QdrantVectorStoreProvider.BuildWorkspaceFilter(new VectorStoreContext
        {
            TenantKey = "t1",
            WorkspaceName = "ws",
            SourceName = "KnowledgeBase",
            MetadataFilters = new Dictionary<string, string> { ["projectId"] = ProjectId.ToString("D") }
        });

        var keys = filter.Must.Select(condition => condition.Field.Key).ToList();
        keys.ShouldBe(new[] { "tenantKey", "workspaceName", "sourceName", "meta.projectId" });
    }

    [Fact]
    public void Keys_Containing_Path_Separators_Should_Be_Normalized_Consistently()
    {
        var payload = QdrantPayloadMetadata.BuildPayloadValue(new Dictionary<string, object>
        {
            ["source.path[0]"] = "value"
        });

        var filterKey = QdrantPayloadMetadata.ToFilterKey("source.path[0]");

        filterKey.ShouldBe("meta.source_path_0_");
        payload.StructValue.Fields.ContainsKey("source_path_0_").ShouldBeTrue();
    }

    /// <summary>
    /// Mirrors how Qdrant resolves a dotted payload key: each segment descends one object level.
    /// </summary>
    private static string? ResolvePath(IReadOnlyDictionary<string, Value> payload, string dottedKey)
    {
        var segments = dottedKey.Split('.');
        if (!payload.TryGetValue(segments[0], out var current))
        {
            return null;
        }

        foreach (var segment in segments.Skip(1))
        {
            if (current.KindCase != Value.KindOneofCase.StructValue ||
                !current.StructValue.Fields.TryGetValue(segment, out var next))
            {
                return null;
            }

            current = next;
        }

        return current.KindCase == Value.KindOneofCase.StringValue ? current.StringValue : null;
    }
}
