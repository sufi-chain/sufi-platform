# AI Management Troubleshooting

Use this page for common AI Management setup issues. For module structure and extension points, see [Architecture](architecture.md).

## Provider or model calls fail

- Confirm workspace provider credentials and endpoint URLs in the admin UI.
- Verify the selected model configuration is enabled for the required capability (chat, embedding, vision, etc.).
- Check host logs for HTTP errors from the provider adapter.

## RAG returns no results

- Confirm documents were ingested and chunks exist for the workspace.
- Verify embedder and vector-store settings match the configured provider.
- Check that the query uses the same workspace scope as the indexed content.

## MCP tools are unavailable

- Confirm the MCP server entry is enabled and reachable from the API host.
- Verify authentication settings for the external MCP endpoint.
- Inspect application logs around MCP connection attempts.

## Blazor admin pages do not load

- Ensure `SufiChain.SufiPlatform.SufiAI.*` packages are referenced and module dependencies are registered in the host.
- Verify the current user has the required AI Management permissions.
- See [Installation](installation.md) for the expected module dependency chain.
