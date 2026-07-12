# AI Management Architecture

AI Management follows Sufi Platform layered module structure with domain-driven design.

## Module packages

| Package | Responsibility |
|---------|----------------|
| `Domain.Shared` | Enums (`AICapabilityType`, `AIProviderType`, `OpenAIApiMode`, `VectorStoreType`), constants, localization |
| `Domain` | Entities, domain services, providers, RAG, MCP |
| `Application.Contracts` | DTOs, app service interfaces, `AIOptions` |
| `Application` | App service implementations, mapping |
| `HttpApi` | `OpenAICompatibleController` |
| `HttpApi.Client` | Client proxies |
| `Blazor` | Admin UI |
| `EntityFrameworkCore` / `MongoDB` | Persistence |

## Domain layer

### Entities

- `Workspace` — aggregate root: provider credentials, chat defaults, embedder/vector JSON, costs
- `AIModelConfiguration` — per-capability model with priority
- `AIUsageLog` — usage and cost tracking
- `MCPServer` — external MCP connection config
- `DocumentChunk` — RAG chunks (content + embedding metadata)

### Domain services

- `AIService` / `IAIService` — orchestrates capabilities via `IAIProvider` and model configuration
- `OpenAIProvider` — only provider implementation today
- `RAGService` / `IRAGService` — document source registry, indexing, semantic search
- `WorkspaceSyncService` — loads workspace from DB and caches `IChatClient`, `Kernel`, embedding generator per workspace name
- `WorkspaceManager` — workspace business rules
- `MCPToolRegistry` — discovers and executes MCP tools

### Repositories

- `IWorkspaceRepository`
- `IAIModelConfigurationRepository`
- `IAIUsageLogRepository`
- `IMCPServerRepository`

## Application layer

| Service | Role |
|---------|------|
| `AIAppService` | Multi-modal operations, model config CRUD, usage queries |
| `WorkspaceAppService` | Workspace CRUD, list models, test connection |
| `AIChatAppService` | Thin chat wrapper |
| `RAGAppService` | RAG search, indexing, source listing |
| `MCPServerAppService` / `MCPToolAppService` | MCP management and execution |
| `AIKernelAppService` | Returns Semantic Kernel for workspace |

Object mapping uses Mapperly (or project-standard mapper) for entity ↔ DTO.

## Infrastructure

### AI providers

- **Implemented:** `OpenAIProvider` (chat stream, audio, vision, embeddings, TTS paths as configured)
- **Not implemented:** Azure OpenAI, Ollama (enum may expand later)

### Vector stores

- **Implemented:** MongoDB via `IVectorStoreProvider` (`VectorStoreType.MongoDB`)
- **Not implemented:** Pgvector, Qdrant

### File storage

- `FileManagerStorageService` when File-Manager module is available
- Blob storage fallback otherwise
- Structure key: `AI` (`AIFileStructureKeys.AI`)

## HTTP API

1. **ABP dynamic API** — conventional controllers generated from `I*AppService` in `Application.Contracts` (standard ABP remote service pattern).
2. **`OpenAICompatibleController`** — explicit routes under `/v1`:
   - `POST /v1/chat/completions` (streaming supported)
   - Embeddings and models endpoints
   - Requires `WorkspaceName` on the request body; uses `IAIKernelAppService`

## Blazor layer

### Pages

All under `Pages/AI/`: Workspaces, ModelConfigurations, TestChat, MultiModalTest, UsageAnalytics, RAG, IndexingStatus, MCPTools, MCPServers.

### Components

- `WorkspaceSelector` — horizontal workspace cards
- `WorkspaceCreateModal` / `WorkspaceEditModal` — tabbed connection + cost
- `ModelConfigurationModal`, `MCPServerModal`

### Base type

`AIComponentBase` — localization, `ExecuteWithLoadingAsync`, `LazyGetRequiredService`.

Pages use `SufiAbpPageToolbar` for actions (platform pattern in this module).

## Key design patterns

### Workspace isolation

Operations are scoped by workspace name and tenant. Usage logs and configurations are tenant-aware.

### Priority-based fallback

`AIService` selects the enabled model with the lowest priority number for the requested capability; tries fallbacks on failure.

### On-demand workspace sync

`WorkspaceSyncService` builds provider clients from DB workspace rows and caches them in static concurrent dictionaries until cache clear/restart.

### RAG document sources

Modules implement `IDocumentSource` (`SourceName`, `SearchAsync`, `GetByIdAsync`, `GetTotalCountAsync`). At host startup, register with `IRAGService.RegisterDocumentSource`. Indexing chunks content and stores vectors in the configured MongoDB collection.

**HelpDesk:** a product knowledge base can register a source such as `KnowledgeBase` and index it through the same document-source extension points described in [Extending](extending.md).

### MCP tool discovery

- Internal: `[MCPTool]` on application service methods
- External: MCP servers registered per workspace

## Dependencies

- Sufi Platform framework (DDD, UI, authorization)
- `Microsoft.SemanticKernel`
- OpenAI / HTTP clients inside `OpenAIProvider`
- MongoDB driver (when MongoDB vector store or MongoDB module is used)
- EF Core (when EF module is used)
- File-Manager (optional)
