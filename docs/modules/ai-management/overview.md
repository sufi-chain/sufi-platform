# AI Management Overview

The AI Management module provides reusable multi-modal AI capabilities for Sufi Platform applications. It is a horizontal infrastructure module: products depend on it for workspaces, model configuration, RAG indexing, usage analytics, and MCP integration instead of calling providers directly.

## What it covers

- **OpenAI-backed workspaces** (additional providers are planned; only OpenAI is implemented today)
- Six AI capabilities: chat completion, audio transcription, text-to-speech, vision analysis, embeddings, and image generation (via model configuration)
- Workspace isolation for multi-tenancy and environment separation
- Model configuration with priority-based fallback per capability
- Usage tracking with token counting and cost estimation
- **RAG** (Retrieval-Augmented Generation) for semantic document search and grounded answers
- **MCP** (Model Context Protocol) for AI function calling and external tool servers
- Optional **File-Manager** integration for uploaded media

## How it fits the platform

Treat AI Management like File-Manager: a shared platform capability. Modules such as HelpDesk Knowledge Base, LiveChat, and Ticketing register document sources, index content into a workspace, and call `IAIAppService` for chat—without embedding OpenAI SDKs in each module.

## Admin UI structure

Under **Administration → AI Management**, pages are grouped as follows:

| Group | Pages | Route prefix |
|-------|--------|--------------|
| (standalone) | Workspaces | `/admin/ai-management/workspaces` |
| Configuration | Model Configurations | `/admin/ai-management/model-configurations` |
| Testing | Test Chat, Multi-Modal Test | `/admin/ai-management/test-chat`, `.../multimodal-test` |
| Analytics | Usage Analytics | `/admin/ai-management/usage-analytics` |
| RAG | RAG Search, Indexing Status | `/admin/ai-management/rag`, `.../indexing-status` |
| MCP | MCP Tools, MCP Servers | `/admin/ai-management/mcp-tools`, `.../mcp-servers` |

Operator workflows: [Usage](usage.md). Integrators: [API](api.md), [Extending](extending.md). Architecture: [Architecture](architecture.md).

## Where to start in source

| Package | Purpose |
|---------|---------|
| `SufiChain.SufiAbp.AIManagement.Blazor` | Admin UI, `AIManagementMenuContributor`, `WorkspaceSelector`, pages |
| `SufiChain.SufiAbp.AIManagement.Application.Contracts` | DTOs and service interfaces |
| `SufiChain.SufiAbp.AIManagement.Application` | Application services |
| `SufiChain.SufiAbp.AIManagement.Domain` | `OpenAIProvider`, `RAGService`, `WorkspaceSyncService`, MCP registry |
| `SufiChain.SufiAbp.AIManagement.HttpApi` | `OpenAICompatibleController` (`/v1/*`) |
| `SufiChain.SufiAbp.AIManagement.EntityFrameworkCore` / `.MongoDB` | Persistence |

Implementation root: `src/modules/ai-management/`.
