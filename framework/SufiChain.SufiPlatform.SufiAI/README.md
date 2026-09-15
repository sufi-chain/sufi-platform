# SufiChain.SufiPlatform.SufiAI

Thin ABP module that depends on `SufiChain.SufiPlatform.SufiAI.Abstractions`.

Database workspaces, kernels, embeddings, and provider execution live in the AI
module (`SufiChain.SufiPlatform.SufiAI.Domain` and application layers). Product
code uses `ISufiAIChatService` and the other DTO-based contracts in
Abstractions. Do not register keyed `IChatClient` or `Kernel` instances from
this package.

## Related packages

- `SufiChain.SufiPlatform.SufiAI.Abstractions` — product contracts
- `SufiChain.SufiPlatform.SufiAI.Domain` — workspace sync, providers, MCP, RAG
