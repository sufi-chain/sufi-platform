using Shouldly;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.RAG;

public class DocumentSourceStatusTests
{
    [Fact]
    public void Name_status_and_last_indexed_come_from_the_indexing_status()
    {
        var dto = new DocumentSourceDto();
        var indexedAt = new DateTime(2026, 11, 27, 10, 0, 0, DateTimeKind.Utc);
        DocumentSourceStatus.Apply(dto, "help", 4, new IndexingStatus
        {
            SourceName = "help",
            TotalDocuments = 4,
            IndexedDocuments = 4,
            LastIndexedAt = indexedAt
        });

        dto.Name.ShouldBe("help");
        dto.SourceName.ShouldBe("help");
        dto.Status.ShouldBe(IndexingStatusType.Complete);
        dto.LastIndexedAt.ShouldBe(indexedAt);
        dto.Progress.ShouldBe(100);
        dto.RequestName.ShouldBe("help");
    }

    [Fact]
    public void Indexing_error_and_pending_stay_distinct()
    {
        var indexing = new DocumentSourceDto();
        DocumentSourceStatus.Apply(indexing, "live", 2, new IndexingStatus
        {
            TotalDocuments = 2,
            IndexedDocuments = 1,
            IsIndexing = true
        });
        indexing.Status.ShouldBe(IndexingStatusType.Indexing);

        var failed = new DocumentSourceDto();
        DocumentSourceStatus.Apply(failed, "broken", 2, new IndexingStatus
        {
            TotalDocuments = 2,
            IndexedDocuments = 0,
            ErrorMessage = "store down"
        });
        failed.Status.ShouldBe(IndexingStatusType.Failed);
        failed.ErrorMessage.ShouldBe("store down");

        var pending = new DocumentSourceDto();
        DocumentSourceStatus.Apply(pending, "waiting", 0, null);
        pending.Name.ShouldBe("waiting");
        pending.Status.ShouldBe(IndexingStatusType.Pending);
        pending.LastIndexedAt.ShouldBeNull();
        pending.RequestName.ShouldBe("waiting");
    }

    [Fact]
    public void Reindex_sends_the_source_name_the_server_fills()
    {
        var page = ReadPage();
        page.ShouldContain("GetDocumentSourcesAsync(workspaceName)");
        page.ShouldContain("StartIndexingAsync(workspace.Name, source.RequestName)");
    }

    private static string ReadPage()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var path = Path.Combine(
                dir.FullName,
                "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/IndexingStatus.razor.cs");
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("IndexingStatus.razor.cs");
    }
}
