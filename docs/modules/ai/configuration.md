# AI Management Configuration

Configure AI Management through module options, workspace records in the database, and host module dependencies.

## Module options (`AIOptions`)

Register in the host module `ConfigureServices`:

```csharp
Configure<AIOptions>(options =>
{
    options.SeedFileStructure = true; // default: true
});
```

When File-Manager is present, call `AddDefaultFileStructure` on `FileManagerOptions` during File-Manager setup:

```csharp
Configure<FileManagerOptions>(fileManagerOptions =>
{
    var aiOptions = context.Services
        .BuildServiceProvider()
        .GetRequiredService<IOptions<AIOptions>>().Value;
    aiOptions.AddDefaultFileStructure(fileManagerOptions);
});
```

| Property | Default | Description |
|----------|---------|-------------|
| `SeedFileStructure` | `true` | Seeds the **AI** file structure in File-Manager when that module is loaded |

There is **no** global OpenAI API key in `appsettings.json` for AI Management. API keys live on **workspace** entities (admin UI or `IWorkspaceAppService`).

## Workspace configuration (database)

Each workspace stores provider and RAG settings. Create/update via admin UI or `CreateWorkspaceDto` / `UpdateWorkspaceDto`.

### Connection and chat

| Field | Description |
|-------|-------------|
| `Name` | Unique workspace name (used in `WorkspaceName` on API inputs) |
| `Provider` | `AIProviderType.OpenAI` (only value today) |
| `Model` | Default chat model ID |
| `ApiKey` | Provider key (write-only on create/update; `HasApiKey` on read DTO) |
| `ApiBaseUrl` | Optional custom base URL |
| `SystemPrompt` | Default system message for chat |
| `Temperature` | 0.0–2.0 |
| `MaxTokens` | Max response tokens |
| `OpenAIApiMode` | `ChatCompletions` or `Responses` |
| `InputCostPer1KTokens` / `OutputCostPer1KTokens` | Optional cost estimation for analytics |
| `IsActive` | Enable/disable workspace |

### RAG (embeddings + vector store)

| Concern | Where |
|---------|--------|
| Embeddings model | Multimodal `AIModelConfiguration` with `AICapabilityType.Embeddings` (Workspace → Model configurations) |
| Vector store | Host `appsettings` — exactly one of `VectorStore:Qdrant` or `VectorStore:Pgvector` |

Default workspace seed creates Chat, Embeddings, Audio, TTS, Vision, and ImageGeneration model rows. Indexing fails with `AI:EmbeddingsModelNotConfigured` if the Embeddings row is missing, or `AI:RagUnavailable` if the host vector store is not configured.

Example: create an Embeddings model after the workspace exists (or rely on default seed):

```csharp
await _aiAppService.CreateModelConfigurationAsync(new CreateAIModelConfigurationDto
{
    WorkspaceId = workspaceId,
    CapabilityType = AICapabilityType.Embeddings,
    ModelId = "text-embedding-3-small",
    Priority = 0,
    IsEnabled = true
});
```

## Model configuration JSON

Per-model optional JSON on `AIModelConfiguration` (examples):

**Chat:**

```json
{
  "temperature": 0.7,
  "max_tokens": 2000,
  "top_p": 1.0
}
```

**Embeddings:**

```json
{
  "dimensions": 1536
}
```

## Host module dependencies

```csharp
[DependsOn(
    typeof(SufiAIApplicationModule),
    typeof(SufiAIHttpApiModule),
    typeof(SufiAIBlazorModule),
    typeof(SufiAIEntityFrameworkCoreModule) // or MongoDB module
)]
public class YourHostModule : AbpModule { }
```

Blazor: add `SufiAIBlazorModule` and ensure `RouterOptions.AdditionalAssemblies` includes the Blazor assembly (module does this in `ConfigureServices`).

## Environment-specific secrets

Store workspace API keys via admin UI, environment-specific seed, or secure configuration applied at deployment—not committed to source control.

For File Manager storage details, document the final integration in the File Manager module docs or in the consuming product docs.

## Consuming modules (HelpDesk)

HelpDesk-style knowledge bases can register `IDocumentSource` with a source name such as `KnowledgeBase`, index through `IRAGAppService.StartIndexingAsync(workspaceName, "KnowledgeBase")`, and answer by combining `SearchDocumentsAsync` with `IAIAppService`.
