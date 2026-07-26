# Extending AI Management

Extension patterns for providers, RAG sources, MCP tools, and UI.

## Consuming modules (HelpDesk)

HelpDesk modules must **not** call OpenAI directly. Use AI Management contracts:

1. **Register a document source** at module initialization:

```csharp
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var ragService = context.ServiceProvider.GetRequiredService<IRAGService>();
    var kbSource = context.ServiceProvider.GetRequiredService<KBArticleDocumentSource>();
    ragService.RegisterDocumentSource(kbSource);
}
```

2. **Index content** after publish or on schedule:

```csharp
await _ragAppService.StartIndexingAsync("helpdesk-default", "KnowledgeBase");
```

3. **Answer with RAG context**:

```csharp
var chunks = await _ragAppService.SearchDocumentsAsync(new SearchDocumentsInput
{
    WorkspaceName = options.DefaultAiWorkspaceName,
    Query = userMessage,
    MaxResults = 5
});
// Pass chunk content into SendChatMessageAsync / StreamChatMessageAsync system prompt
```

**Conventions:**

| Item | Value |
|------|--------|
| KB RAG source name | `KnowledgeBase` |
| Default workspace | Host setting `DefaultAiWorkspaceName` (e.g. `helpdesk-default`) |

Keep product-specific Knowledge Base and Live Chat implementation plans in the consuming product documentation. This page should document the reusable AI Management extension points.

### IDocumentSource

```csharp
public interface IDocumentSource
{
    string SourceName { get; }
    Task<List<DocumentChunk>> SearchAsync(string? query = null, int maxResults = 100, CancellationToken cancellationToken = default);
    Task<DocumentChunk?> GetByIdAsync(string documentId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
```

Return published content only. Populate `Metadata` (title, slug, url) for citations in UI.

## Custom AI providers

Implement `IAIProvider` in the Domain layer and register with DI. Today only `OpenAIProvider` is registered; additional providers require enum extension and UI work.

Use `context.Services.Replace(ServiceDescriptor.Transient<IAIProvider, YourProvider>())` only when replacing a specific registration pattern—follow Sufi Platform module override rules.

## Internal MCP tools

Mark application service methods with `[SufiAiMcpTool]`:

```csharp
public class CalculatorAppService : SufiApplicationService
{
    [SufiAiMcpTool("add", "Add two numbers")]
    public virtual Task<int> AddAsync(int a, int b) => Task.FromResult(a + b);
}
```

Refresh registry via `IMCPToolAppService.RefreshToolRegistryAsync()` when tools change at runtime.

## Custom vector stores

Implement `IVectorStoreProvider` with a distinct `VectorStoreType` and register in DI. MongoDB is the built-in implementation used by `RAGService`.

## Extending Blazor UI

Add pages in your module assembly; inherit `AIComponentBase` only if referencing the Blazor package is acceptable, otherwise use your module base.

```razor
@page "/admin/my-feature/ai-dashboard"
@attribute [Authorize(MyPermissions.Default)]
@inherits MyModuleComponentBase
```

Add menu items via `IMenuContributor`:

```csharp
var aiMenu = context.Menu.FindMenuItem(AIMenus.GroupName);
aiMenu?.AddItem(new ApplicationMenuItem(
    "MyModule.AiDashboard",
    l["AiDashboard"],
    url: "/admin/my-feature/ai-dashboard",
    icon: "chart"
).RequirePermissions(MyPermissions.Default));
```

Use code-behind (`.razor.cs`); no `@code` blocks.

## Custom usage cost calculation

Replace `IUsageCostCalculator` (if registered) using:

```csharp
context.Services.Replace(ServiceDescriptor.Transient<IUsageCostCalculator, CustomCostCalculator>());
```

## Distributed events

Subscribe to AI-related ETOs from your module (e.g. HelpDesk `ArticlePublishedEto` → trigger `StartIndexingAsync`). Handlers may live in the HelpDesk host or a thin adapter in AI Management—prefer HelpDesk-owned handlers to avoid coupling AI to HelpDesk types.

## Best practices

- Virtual methods on app services in reusable modules
- Localize user-facing strings
- Test indexing and search in AI UI before product launch
- Document your `SourceName` constant for operators
