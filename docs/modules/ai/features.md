# AI Management Features

Map product requirements to AI Management capabilities before building a custom integration.

## Admin UI (by menu group)

### Workspaces

- **Route:** `/admin/ai/workspaces`
- Create and edit workspaces (OpenAI provider in UI today)
- Load available models from provider API, test connection before save
- Configure default chat model, API key, base URL, optional cost per 1M tokens
- Chat protocol (`OpenAIApiMode`), context window, display name, and user-selectability live on each `AIModelConfiguration`
- RAG embeddings use multimodal `AIModelConfiguration` (Embeddings); vector store is host `VectorStore:*` only. MCP/RAG authorization is copilot policy, not workspace configuration.

### Configuration

- **Model Configurations** (`/admin/ai/model-configurations`)
- Per-workspace models per capability (Chat, Audio, Vision, Embeddings, TTS, Image Generation)
- Priority-based fallback, enable/disable without delete

### Testing

- **Test Chat** (`/admin/ai/test-chat`) — conversational UI with optional streaming, token/latency display
- **Multi-Modal Test** (`/admin/ai/multimodal-test`) — tabs for chat, audio transcription, TTS, vision, embeddings (tabs respect capability permissions)

### Analytics

- **Usage Analytics** (`/admin/ai/usage-analytics`) — cost, tokens, requests, success rate, breakdowns, recent logs, date range filter

### RAG

- **RAG Search** (`/admin/ai/rag`) — semantic search over indexed document sources for a workspace
- **Indexing Status** (`/admin/ai/indexing-status`) — list sources, start/re-index, monitor progress

### MCP

- **MCP Tools** (`/admin/ai/mcp-tools`) — browse tools, view JSON schema (execution via API)
- **MCP Servers** (`/admin/ai/mcp-servers`) — CRUD, enable/disable, **test connection** per server

## Developer-facing capabilities

Application services (also exposed as ABP dynamic HTTP API unless disabled):

| Service | Purpose |
|---------|---------|
| `IAIAppService` | Chat (incl. stream), audio, vision, embeddings, model configs, usage stats (including per-route cost) |
| `IWorkspaceAppService` | Workspace CRUD, cached list models, test connection |
| `IAIModelCatalogAppService` | Selectable routes for the composer picker |
| `IAIChatAppService` | Simplified single-message chat |
| `IRAGAppService` | Document sources list, search, indexing status, start indexing |
| `IMCPServerAppService` | MCP server CRUD, test connection |
| `IMCPToolAppService` | List tools, execute tool, refresh registry |

**OpenAI-compatible HTTP API:** `POST /v1/chat/completions`, embeddings, and `GET /v1/models` via `OpenAICompatibleController`. Chat requires `AIPermissions.AI.Chat` and a `WorkspaceName`. Model names map to ready workspace routes; there is no hardcoded llama/mistral list. See [API](api.md).

## AI capabilities

| Capability | Typical models | Notes |
|------------|----------------|-------|
| Chat Completion | GPT-4, GPT-3.5 | Streaming supported |
| Audio Transcription | Whisper | File upload |
| Text-to-Speech | OpenAI TTS | Play/download in test UI |
| Vision Analysis | GPT-4 Vision | Image + prompt |
| Embeddings | text-embedding-3-small | Used for RAG |
| Image Generation | DALL-E | Model config supported; limited test UI |

## File storage integration

When File-Manager is installed and `AIOptions.SeedFileStructure` is enabled, the module seeds an **AI** file structure (images, audio, video, documents) for workspace uploads. Without File-Manager, blob storage fallback applies.

## Implemented vs planned

| Area | Status |
|------|--------|
| OpenAI provider | Implemented |
| Azure OpenAI, Ollama, custom providers | Not implemented |
| RAG with MongoDB vector store | Implemented |
| Pgvector, Qdrant | Not implemented |
| RAG embedder/vector admin UI | Partial (workspace DTO/API; no dedicated RAG settings page) |
| Document source CRUD UI | Not implemented (sources registered in code) |
| MCP tool execute UI | Not implemented (API only) |

## Platform role

Shared AI platform for chat, voice, vision, and document intelligence across business modules. HelpDesk and similar products should use RAG + `IAIAppService` rather than duplicate provider integrations.
