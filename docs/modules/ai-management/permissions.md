# AI Management Permissions

Granular permissions control admin UI and API access.

## Permission groups

### Workspaces

- `AIManagement.Workspaces.Default` — View workspaces and OpenAI `/v1` endpoints using workspace default policy
- `AIManagement.Workspaces.Create`
- `AIManagement.Workspaces.Edit`
- `AIManagement.Workspaces.Delete`

### AI operations

- `AIManagement.AI.Default` — Base AI access
- `AIManagement.AI.Chat` — Chat and Multi-Modal Test chat tab
- `AIManagement.AI.Audio` — Audio transcription and TTS
- `AIManagement.AI.Vision` — Vision analysis
- `AIManagement.AI.Embeddings` — Embeddings generation
- `AIManagement.AI.FunctionCalling` — MCP function calling
- `AIManagement.AI.ManageConfigurations` — Model configuration UI
- `AIManagement.AI.ViewUsage` — Usage analytics

### RAG

- `AIManagement.RAG.Default` — RAG search and indexing status pages
- `AIManagement.RAG.Manage` — Manage RAG configuration (reserved for future UI)
- `AIManagement.RAG.Index` — Start indexing operations

### Test Chat

- `AIManagement.TestChat.Default` — Test Chat page and Testing menu group visibility

### MCP tools

- `AIManagement.MCPTools.Default` — View MCP tools
- `AIManagement.MCPTools.Execute` — Execute tools via API
- `AIManagement.MCPTools.Manage` — Manage tool configuration

### MCP servers

- `AIManagement.MCPServers.Default` — View servers
- `AIManagement.MCPServers.Create`
- `AIManagement.MCPServers.Edit`
- `AIManagement.MCPServers.Delete`

## Page and route matrix

| Route | Page | Permission |
|-------|------|------------|
| `/admin/ai-management/workspaces` | Workspaces | `Workspaces.Default` (create/edit/delete on actions) |
| `/admin/ai-management/model-configurations` | Model Configurations | `AI.ManageConfigurations` |
| `/admin/ai-management/test-chat` | Test Chat | `TestChat.Default` |
| `/admin/ai-management/multimodal-test` | Multi-Modal Test | Menu: `TestChat.Default`; tabs: `AI.Chat`, `.Audio`, `.Vision`, `.Embeddings` |
| `/admin/ai-management/usage-analytics` | Usage Analytics | `AI.ViewUsage` |
| `/admin/ai-management/rag` | RAG Search | `RAG.Default` |
| `/admin/ai-management/indexing-status` | Indexing Status | `RAG.Default` |
| `/admin/ai-management/mcp-tools` | MCP Tools | `MCPTools.Default` |
| `/admin/ai-management/mcp-servers` | MCP Servers | `MCPServers.Default` (+ create/edit/delete) |

Top-level **AI Management** menu requires `Workspaces.Default`.

## Hierarchy

```
AIManagement
├── Workspaces (Default, Create, Edit, Delete)
├── AI (Default, Chat, Audio, Vision, Embeddings, FunctionCalling, ManageConfigurations, ViewUsage)
├── RAG (Default, Manage, Index)
├── TestChat (Default)
├── MCPTools (Default, Execute, Manage)
└── MCPServers (Default, Create, Edit, Delete)
```

## Typical roles

### Administrator

Grant all `AIManagement.*` permissions.

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
[Authorize(AIManagementPermissions.Workspaces.Create)]
public virtual async Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input)
```

### Blazor pages

```razor
@attribute [Authorize(Policy = AIManagementPermissions.Workspaces.Default)]
@inherits AIManagementComponentBase
```

### UI actions

```razor
<AuthorizeView Policy="@AIManagementPermissions.Workspaces.Create">
    <SbButton OnClick="@OpenCreateModal">...</SbButton>
</AuthorizeView>
```

### Programmatic

```csharp
if (await AuthorizationService.IsGrantedAsync(AIManagementPermissions.AI.Chat))
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
