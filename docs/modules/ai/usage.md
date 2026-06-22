# AI Management Usage

Operator workflows for the AI Management admin UI. Routes live under `/admin/ai/`.

## Navigation

Open **Administration → AI Management**. The menu contains:

1. **Workspaces**
2. **Configuration → Model Configurations**
3. **Testing → Test Chat**, **Multi-Modal Test**
4. **Analytics → Usage Analytics**
5. **RAG → RAG Search**, **Indexing Status**
6. **MCP → MCP Tools**, **MCP Servers**

Grant permissions per [Permissions](permissions.md) before expecting menu items to appear.

## Creating and editing workspaces

**Path:** `/admin/ai/workspaces`

1. Click **New Workspace** (requires create permission).
2. In the create modal (**Connection** tab):
   - **Name** — unique identifier used as `WorkspaceName` in API calls (e.g. `helpdesk-default`)
   - **Model** — select from list or type custom ID; use **Load models** after entering API key and base URL
   - **API Key** and **API Base URL** — OpenAI credentials
   - **OpenAI API mode** — Chat Completions (default) or Responses; Responses shows a compatibility warning
3. **Generation and cost** tab (create/edit):
   - **Temperature**, **Max tokens**
   - **Input / output cost per 1K tokens** — optional, for usage analytics estimates
4. Click **Test connection** to verify credentials before **Save**.
5. On the grid, use **Edit** / **Delete** as permitted.

**Edit modal:** API key field can be left empty to keep the existing key (`HasApiKey` on the DTO indicates a key is stored).

**RAG readiness:** Embedder and vector store configuration are stored on the workspace entity (`EmbedderConfig`, `VectorStoreConfig` on create/update DTOs). The grid shows whether configs exist via `HasEmbedderConfig` / `HasVectorStoreConfig`. Until a dedicated RAG settings UI exists, configure these via API or direct update. RAG search and indexing require both configs plus a successful index run (see [Indexing](#managing-document-indexing)).

> **Provider note:** The UI currently supports **OpenAI only**. Additional providers are not selectable yet.

## Configuring models

**Path:** `/admin/ai/model-configurations`

1. Select a workspace (workspace selector or filter).
2. Click **New Model Configuration**.
3. Set **Capability**, **Model ID**, optional endpoint/key override, **Priority** (0 = highest), optional **Configuration JSON**.
4. Use the enable toggle to disable a model without deleting it.

Example priorities for one workspace:

- Chat: `gpt-4` (0), `gpt-3.5-turbo` (1)
- Audio: `whisper-1` (0)
- Embeddings: `text-embedding-3-small` (0)

## Test Chat

**Path:** `/admin/ai/test-chat`  
**Permission:** `AI.TestChat.Default`

1. Select a workspace in the **workspace selector** (horizontal cards).
2. Optional: enable **Use streaming** for token-by-token responses.
3. Type a message and send.
4. View the transcript (user vs assistant). Assistant messages can show **tokens** and **latency** chips when returned by the API.
5. **Clear chat** resets the conversation.

Use this page to validate workspace credentials and chat models before wiring product features (e.g. HelpDesk LiveChat).

## Multi-Modal Test

**Path:** `/admin/ai/multimodal-test`  
**Permission:** Testing menu requires `AI.TestChat.Default`; each capability tab requires its own permission (`AI.AI.Chat`, `.Audio`, `.Vision`, `.Embeddings`).

1. Select a workspace.
2. Open a capability tab (only tabs you are allowed to see are shown):
   - **Chat** — message in, response with usage metadata
   - **Audio transcription** — upload audio, transcribe
   - **Text-to-speech** — enter text, generate and play/download audio
   - **Vision** — upload image + prompt, analyze
   - **Embeddings** — enter text, view dimensions and sample values
3. **Reset** clears the current tab’s form and results.

## Usage analytics

**Path:** `/admin/ai/usage-analytics`  
**Permission:** `AI.AI.ViewUsage`

1. Select a workspace.
2. Set **Date range** (optional).
3. Review summary cards: total cost, tokens, requests, success rate.
4. Review breakdowns by capability and model.
5. Scroll to **recent usage logs** (timestamp, capability, model, tokens, cost, latency, status, errors).
6. **Refresh** reloads data.

## Searching documents with RAG

**Path:** `/admin/ai/rag`  
**Permission:** `AI.RAG.Default`

1. Select a workspace that has embedder + vector store configured and indexed sources.
2. Enter a natural-language **query**.
3. Click **Search**.
4. Results show source name, text chunk, similarity score, and metadata (e.g. title, slug when provided by the document source).

If no results appear, confirm indexing completed on [Indexing Status](#managing-document-indexing).

## Managing document indexing

**Path:** `/admin/ai/indexing-status`  
**Permission:** `AI.RAG.Default` (start indexing may require `AI.RAG.Index` when enforced on the action)

1. Select a workspace.
2. View registered **document sources** (e.g. `KnowledgeBase` from HelpDesk KB).
3. Note document count, last indexed time, and status (Pending, Indexing, Complete, Failed).
4. Click **Start indexing** to index or re-index a source.
5. Monitor progress (progress indicator when available).

Document sources are registered by consuming modules at startup via `IRAGService.RegisterDocumentSource`. Chunking and embedding run inside AI Management during indexing.

## Managing MCP servers

**Path:** `/admin/ai/mcp-servers`  
**Permissions:** view `AI.MCPServers.Default`; create/edit/delete as named

1. Select or filter by workspace.
2. **New MCP server** — name, transport (STDIO, SSE, HTTP), command/arguments or endpoint, metadata JSON.
3. **Test connection** on a row (icon button) — verifies connectivity; shows success/error message.
4. Enable/disable without deleting.

## Browsing MCP tools

**Path:** `/admin/ai/mcp-tools`

1. Select a workspace.
2. Browse internal and external tools (name, description, type, source).
3. **View schema** opens parameter JSON schema.
4. Tool **execution** is available through `IMCPToolAppService.ExecuteToolAsync` (API), not the admin UI yet.

## Typical validation flow

1. Create workspace → **Test connection**
2. Add model configurations per capability
3. **Test Chat** (with streaming)
4. Configure RAG on workspace → **Indexing Status** → index source → **RAG Search**
5. Optional: MCP server **Test connection**

## Why it matters

These pages let operators configure, test, and monitor AI without code changes. Product modules (HelpDesk, etc.) reuse the same workspaces and RAG indexes configured here.
