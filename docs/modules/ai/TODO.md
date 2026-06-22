# AI Management — Roadmap

Tracks admin UI and platform gaps. Backend support may exist before UI.

**Last updated:** 2026-05-17

## Completed

- Workspaces CRUD with tabbed create/edit modals
- Workspace **Test connection** and **Load models**
- OpenAI API mode (Chat Completions / Responses) on workspace
- Cost per 1K tokens on workspace for analytics
- Model configurations CRUD with enable toggle and priority
- **Test Chat** with streaming toggle and usage chips
- **Multi-Modal Test** with permission-gated capability tabs
- **Usage analytics** with date range and summary cards
- **RAG Search** and **Indexing Status** pages
- **MCP Servers** CRUD with **Test connection** on grid
- **MCP Tools** browser with view schema
- Grouped admin menu (Configuration, Testing, Analytics, RAG, MCP)
- `OpenAICompatibleController` (`/v1/chat/completions`, embeddings, models)
- `WorkspaceSyncService` on-demand client/kernel cache
- File-Manager AI structure seeding via `AIOptions`

## Remaining

### Priority 1

| Item | Notes |
|------|--------|
| RAG configuration UI | Edit embedder/vector JSON per workspace without API |
| Document source management UI | List/register sources, trigger re-index from one place |
| MCP tool execution UI | Dynamic form from schema, show results |

### Priority 2

| Item | Notes |
|------|--------|
| Usage analytics charts | Trends over time; export CSV |
| Usage log pagination | Beyond current recent-log limit |
| Advanced model testing | Test model before save from model config modal |

### Priority 3

| Item | Notes |
|------|--------|
| Additional providers | Azure OpenAI, Ollama, custom |
| Additional vector stores | Pgvector, Qdrant |
| Workspace templates | One-click multi-capability presets |
| Cost budgets and alerts | Per workspace/tenant |
| Audit log viewer | Detailed AI operation audit UI |

## Documentation

When shipping features above, update:

- [usage.md](usage.md)
- [features.md](features.md)
- [Roadmap](TODO.md) (this file)

## HelpDesk dependency

HelpDesk KB/LiveChat/Ticketing plans rely on:

- Workspace + RAG config
- `KnowledgeBase` document source indexing
- `IAIAppService` / `IRAGAppService`

See repo `docs/HELPDESK-*.md` plans.
