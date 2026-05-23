# AI Management Settings

Runtime settings for AI Management are stored primarily on **workspace** and **model configuration** entities, not in a separate settings UI.

## Workspace settings

### Basic

| Setting | Description |
|---------|-------------|
| Name | Unique per tenant; API `WorkspaceName` |
| Provider | OpenAI (only implemented provider) |
| Model | Default chat model |
| API key | Stored encrypted/hidden; `HasApiKey` on DTO |
| API base URL | Optional |
| Is active | Soft enable/disable |

### Generation

| Setting | Description |
|---------|-------------|
| System prompt | Default system message |
| Temperature | Randomness |
| Max tokens | Response limit |
| OpenAI API mode | Chat Completions vs Responses |

### Cost tracking

| Setting | Description |
|---------|-------------|
| Input cost per 1K tokens | Optional USD estimate |
| Output cost per 1K tokens | Optional USD estimate |

Used by usage analytics when operations are logged.

### RAG

| Setting | Storage |
|---------|---------|
| Embedder config | `EmbedderConfigJson` on workspace |
| Vector store config | `VectorStoreConfigJson` on workspace |

DTO flags: `HasEmbedderConfig`, `HasVectorStoreConfig`.

## Model configuration settings

| Setting | Description |
|---------|-------------|
| Capability type | Chat, Audio, Vision, Embeddings, TTS, Image Generation |
| Model ID | Provider model name |
| API endpoint / key | Optional overrides |
| Is enabled | Toggle without delete |
| Priority | Fallback order (0 = first) |
| Configuration JSON | Provider-specific parameters |

## MCP server settings

| Setting | Description |
|---------|-------------|
| Name | Server identifier |
| Workspace ID | Parent workspace |
| Transport | STDIO, SSE, HTTP |
| Endpoint / command / arguments | Connection details |
| Is enabled | Active flag |
| Metadata JSON | Headers, env vars, auth |

## Module-level options

| Setting | Location | Default |
|---------|----------|---------|
| Seed File-Manager structure | `AIManagementOptions.SeedFileStructure` | `true` |

## System behavior (not appsettings)

| Behavior | Description |
|----------|-------------|
| Usage logging | Automatic on `IAIAppService` operations |
| Workspace sync cache | `WorkspaceSyncService` caches chat client, kernel, embedder per workspace name |
| Document sources | Registered in code via `IRAGService.RegisterDocumentSource` |

## Updating settings

| Method | When to use |
|--------|-------------|
| Admin UI | Workspaces, model configs, MCP servers |
| Application services | Automation, HelpDesk host setup |
| Database | Avoid except migrations |

Changes to workspace keys and models take effect on next request; clear workspace sync cache is handled internally when configs change (restart host if you customize caching).

## Best practices

- Separate workspaces per environment (`dev`, `staging`, `production`)
- Configure embedder dimensions to match the embedding model
- Re-index RAG sources after bulk KB updates
- Set cost fields for meaningful analytics dashboards
