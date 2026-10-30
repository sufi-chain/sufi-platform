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
| Embeddings model | `AIModelConfiguration` with `AICapabilityType.Embeddings` (Workspace → Model configurations) |
| Vector store | Host `VectorStore:Qdrant` or `VectorStore:Pgvector` (exactly one) |

## Model configuration settings

| Setting | Description |
|---------|-------------|
| Capability type | Chat, Audio, Vision, Embeddings, TTS, Image Generation |
| Model ID | Provider model name |
| API endpoint / key | Optional overrides |
| Is enabled | Toggle without delete |
| Priority | Fallback order (0 = first) |
| Configuration JSON | Optional provider-specific parameters on the model row |

## MCP server settings

| Setting | Description |
|---------|-------------|
| Name | Server identifier |
| Workspace ID | Parent workspace |
| Transport | STDIO, SSE, HTTP |
| Endpoint / command / arguments | Connection details |
| Is enabled | Active flag |
| Metadata JSON | Headers, env vars, auth |

## LLM gateway (tenant settings)

Chat completions retry only inside `LlmChatGateway`. The OpenAI client stays at `maxRetries: 0`. Each name below is an ABP setting. A tenant value replaces the host default. The prefix is `SufiAI.LlmGateway.`.

| Setting | Default | Meaning |
|---------|---------|---------|
| MaxAttempts | 4 | Tries per call, clamped to 3–5. Only 429, 5xx, timeouts, and connection resets are retried. |
| BaseDelayMs | 200 | Full-jitter base. The wait is `random(0, min(cap, base * 2^attempt))`. |
| MaxDelayMs | 8000 | Cap for that jitter. A `Retry-After` value is used instead of the jitter. |
| RetryBudgetPercent | 10 | About this percent of requests may be retries. |
| BreakerErrorRatePercent | 50 | Opens the provider/workspace/model breaker when the recent window reaches this error rate. |
| BreakerMinimumSamples | 4 | Samples required before the breaker can open. |
| BreakerWindow | 8 | How many recent outcomes the error rate uses. |
| BreakerOpenSeconds | 30 | Open period. The next call is one half-open probe. |
| MaxToolRounds | 10 | Tool-call rounds in one turn. |
| MaxTurnSeconds | 0 | Wall-clock bound for a tool turn. 0 does not start a timer. A positive value is opt-in. |

4xx, content-filter, and validation failures are not retried. Failover uses the next enabled chat model on the same workspace. A stream fails over only before the first text chunk. After that the client receives finish reason `interrupted` and the tool call is not replayed.

## Module-level options

| Setting | Location | Default |
|---------|----------|---------|
| Seed File-Manager structure | `AIOptions.SeedFileStructure` | `true` |

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
