# AI Management Permissions

Granular permissions control admin UI and API access.

## Permission groups

### Workspaces

- `AI.Workspaces.Default` — View workspaces and OpenAI `/v1` endpoints using workspace default policy
- `AI.Workspaces.Create`
- `AI.Workspaces.Edit`
- `AI.Workspaces.Delete`

### AI operations

- `AI.AI.Default` — Base AI access
- `AI.AI.Chat` — Chat and Multi-Modal Test chat tab
- `AI.AI.Audio` — Audio transcription and TTS
- `AI.AI.Vision` — Vision analysis
- `AI.AI.Embeddings` — Embeddings generation
- `AI.AI.FunctionCalling` — MCP function calling
- `AI.AI.ManageConfigurations` — Model configuration UI
- `AI.AI.ViewUsage` — Usage analytics

### RAG

- `AI.RAG.Default` — RAG search and indexing status pages
- `AI.RAG.Manage` — Manage RAG configuration (reserved for future UI)
- `AI.RAG.Index` — Start indexing operations

### Test Chat

- `AI.TestChat.Default` — Test Chat page and Testing menu group visibility

### MCP tools

- `AI.MCPTools.Default` — View MCP tools
- `AI.MCPTools.Execute` — Execute tools via API
- `AI.MCPTools.Manage` — Manage tool configuration

### MCP servers

- `AI.MCPServers.Default` — View servers
- `AI.MCPServers.Create`
- `AI.MCPServers.Edit`
- `AI.MCPServers.Delete`

## Page and route matrix

| Route | Page | Permission |
|-------|------|------------|
| `/admin/ai/workspaces` | Workspaces | `Workspaces.Default` (create/edit/delete on actions) |
| `/admin/ai/model-configurations` | Model Configurations | `AI.ManageConfigurations` |
| `/admin/ai/test-chat` | Test Chat | `TestChat.Default` |
| `/admin/ai/multimodal-test` | Multi-Modal Test | Menu: `TestChat.Default`; tabs: `AI.Chat`, `.Audio`, `.Vision`, `.Embeddings` |
| `/admin/ai/usage-analytics` | Usage Analytics | `AI.ViewUsage` |
| `/admin/ai/rag` | RAG Search | `RAG.Default` |
| `/admin/ai/indexing-status` | Indexing Status | `RAG.Default` |
| `/admin/ai/mcp-tools` | MCP Tools | `MCPTools.Default` |
| `/admin/ai/mcp-servers` | MCP Servers | `MCPServers.Default` (+ create/edit/delete) |

Top-level **AI Management** menu requires `Workspaces.Default`.

## Hierarchy

```
AI
├── Workspaces (Default, Create, Edit, Delete)
├── AI (Default, Chat, Audio, Vision, Embeddings, FunctionCalling, ManageConfigurations, ViewUsage)
├── RAG (Default, Manage, Index)
├── TestChat (Default)
├── MCPTools (Default, Execute, Manage)
└── MCPServers (Default, Create, Edit, Delete)
```

## Typical roles

### Administrator

Grant all `AI.*` permissions.

### AI operator

- `Workspaces.Default`, `Create`, `Edit`
- `AI.ManageConfigurations`, `AI.ViewUsage`
- `TestChat.Default`, `RAG.Default`, `MCPServers.*`

### Developer / integrator

- `Workspaces.Default`
- `AI.Chat`, `AI.Audio`, `AI.Vision`, `AI.Embeddings`
- `TestChat.Default`, `RAG.Default`, `MCPTools.Default`, `MCPTools.Execute`

### End user (via product module)

Grant only what the product needs (e.g. `AI.Chat` for chatbot features backed by app services).

## Permission checking

### Application services

```csharp
[Authorize(AIPermissions.Workspaces.Create)]
public virtual async Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input)
```

### Blazor pages

```razor
@attribute [Authorize(Policy = AIPermissions.Workspaces.Default)]
@inherits AIComponentBase
```

### UI actions

```razor
<AuthorizeView Policy="@AIPermissions.Workspaces.Create">
    <SbButton OnClick="@OpenCreateModal">...</SbButton>
</AuthorizeView>
```

### Programmatic

```csharp
if (await AuthorizationService.IsGrantedAsync(AIPermissions.AI.Chat))
{
    // allowed
}
```

## Multi-tenancy

Permissions are evaluated in the current tenant context. Workspaces, logs, and indexes are tenant-isolated.

## Best practices

- Grant least privilege per role
- Separate configuration permissions from usage permissions
- Use `MCPTools.Execute` only for trusted automation accounts
