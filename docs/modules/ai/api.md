# AI Management API

Integrate AI Management through application services (recommended) or the OpenAI-compatible HTTP surface.

## Application services

### IAIAppService

Multi-modal AI operations.

**Chat:**

```csharp
Task<ChatResponseDto> SendChatMessageAsync(SendChatMessageInput input);
IAsyncEnumerable<ChatResponseDto> StreamChatMessageAsync(SendChatMessageInput input);
```

**Audio:**

```csharp
Task<AudioTranscriptionDto> TranscribeAudioAsync(TranscribeAudioInput input);
Task<TextToSpeechDto> GenerateSpeechAsync(GenerateSpeechInput input);
```

**Vision:**

```csharp
Task<VisionAnalysisDto> AnalyzeImageAsync(AnalyzeImageInput input);
```

**Embeddings:**

```csharp
Task<EmbeddingsDto> GenerateEmbeddingsAsync(GenerateEmbeddingsInput input);
```

**Capability check:**

```csharp
Task<bool> HasCapabilityAsync(string workspaceName, AICapabilityType capabilityType);
```

**Model configuration:**

```csharp
Task<List<AIModelConfigurationDto>> GetModelConfigurationsAsync(Guid workspaceId);
Task<AIModelConfigurationDto> CreateModelConfigurationAsync(CreateAIModelConfigurationDto input);
Task<AIModelConfigurationDto> UpdateModelConfigurationAsync(Guid id, UpdateAIModelConfigurationDto input);
Task DeleteModelConfigurationAsync(Guid id);
```

**Usage:**

```csharp
Task<List<AIUsageLogDto>> GetUsageLogsAsync(Guid workspaceId, DateTime? startDate = null, DateTime? endDate = null);
Task<UsageStatisticsDto> GetUsageStatisticsAsync(Guid workspaceId, DateTime? startDate = null, DateTime? endDate = null);
```

`SendChatMessageInput` includes `WorkspaceName`, `Message`, optional `SystemPrompt`, and streaming flags per contract in `Application.Contracts`.

### IWorkspaceAppService

```csharp
Task<PagedResultDto<WorkspaceDto>> GetListAsync(PagedAndSortedResultRequestDto input);
Task<WorkspaceDto> GetAsync(Guid id);
Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input);
Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input);
Task DeleteAsync(Guid id);
Task<List<OpenAIModelDto>> GetAvailableModelsAsync(GetOpenAIModelsInput input);
Task TestConnectionAsync(TestWorkspaceConnectionInput input);
```

`CreateWorkspaceDto` / `UpdateWorkspaceDto` support `EmbedderConfig` and `VectorStoreConfig` for RAG.

### IRAGAppService

```csharp
Task<List<DocumentSourceDto>> GetDocumentSourcesAsync();
Task<List<DocumentChunkDto>> SearchDocumentsAsync(SearchDocumentsInput input);
Task<IndexingStatusDto> GetIndexingStatusAsync(string workspaceName, string sourceName);
Task StartIndexingAsync(string workspaceName, string sourceName);
```

`SearchDocumentsInput`: `WorkspaceName`, `Query`, `MaxResults` (1–100, default 10).

### IMCPServerAppService

```csharp
Task<List<MCPServerDto>> GetByWorkspaceAsync(Guid workspaceId);
Task<MCPServerDto> GetAsync(Guid id);
Task<MCPServerDto> CreateAsync(CreateMCPServerDto input);
Task<MCPServerDto> UpdateAsync(Guid id, UpdateMCPServerDto input);
Task DeleteAsync(Guid id);
Task EnableAsync(Guid id);
Task DisableAsync(Guid id);
Task<bool> TestConnectionAsync(Guid id);
```

### IMCPToolAppService

```csharp
Task<List<MCPToolDto>> GetToolsForWorkspaceAsync(string workspaceName);
Task<MCPToolDto> GetToolAsync(string workspaceName, string toolName);
Task<MCPToolExecutionResultDto> ExecuteToolAsync(MCPToolExecutionRequestDto request);
Task RefreshToolRegistryAsync();
```

### IAIChatAppService

```csharp
Task<ChatResponseDto> SendMessageAsync(SendChatMessageInput input);
```

### IAIKernelAppService

```csharp
Task<object> GetKernelAsync(string workspaceName, CancellationToken cancellationToken = default);
```

Cast result to `Microsoft.SemanticKernel.Kernel` in advanced scenarios.

## HTTP API

### ABP dynamic API

Application services implementing `IApplicationService` are exposed as REST endpoints by ABP conventions (area `ai`, route names derived from service/method names). Use HTTP API client proxies or OpenAPI from the host.

### OpenAICompatibleController

Explicit OpenAI-style routes (see `SufiChain.SufiAbp.AI.HttpApi`):

| Method | Route | Notes |
|--------|-------|-------|
| POST | `/v1/chat/completions` | `WorkspaceName` required on body; `stream` supported |
| POST | `/v1/embeddings` | Workspace-scoped embeddings |
| GET | `/v1/models` | Lists models for workspace |

Authorization uses `AIPermissions.Workspaces.Default` on chat completions. Pass the configured workspace **name** (not GUID) on requests.

## Usage examples

### Basic chat

```csharp
var response = await _aiAppService.SendChatMessageAsync(new SendChatMessageInput
{
    WorkspaceName = "helpdesk-default",
    Message = "Hello"
});
```

### Streaming chat (e.g. LiveChat)

```csharp
await foreach (var chunk in _aiAppService.StreamChatMessageAsync(new SendChatMessageInput
{
    WorkspaceName = "helpdesk-default",
    Message = userMessage,
    Stream = true
}))
{
    // append chunk.Message
}
```

### RAG-grounded answer (HelpDesk pattern)

```csharp
var chunks = await _ragAppService.SearchDocumentsAsync(new SearchDocumentsInput
{
    WorkspaceName = "helpdesk-default",
    Query = userQuestion,
    MaxResults = 5
});

var context = string.Join("\n\n", chunks.Select(c => c.Content));

await foreach (var chunk in _aiAppService.StreamChatMessageAsync(new SendChatMessageInput
{
    WorkspaceName = "helpdesk-default",
    Message = userQuestion,
    SystemPrompt = $"Use this knowledge base context:\n{context}",
    Stream = true
}))
{
    // deliver to client
}
```

### Start KB indexing

```csharp
await _ragAppService.StartIndexingAsync("helpdesk-default", "KnowledgeBase");
```

### MCP tool execution

```csharp
var result = await _mcpToolAppService.ExecuteToolAsync(new MCPToolExecutionRequestDto
{
    WorkspaceName = "helpdesk-default",
    ToolName = "get_weather",
    Arguments = new Dictionary<string, object> { ["location"] = "Seattle" }
});
```

## Notes

- All operations are workspace- and tenant-scoped.
- Usage is logged automatically for `IAIAppService` calls where implemented.
- Media uploads use File-Manager structure when available.
- Do not call OpenAI directly from feature modules; use these contracts only.
