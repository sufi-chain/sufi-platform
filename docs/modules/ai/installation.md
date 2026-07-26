# AI Management Installation

Add AI Management to a Sufi Platform host application.

## Prerequisites

- .NET 10.0 SDK
- Sufi Platform host (Blazor Server or WebAssembly with server API)
- SQL or MongoDB (match your host persistence)
- OpenAI API access (workspace keys configured in admin UI)
- Optional: File-Manager module for structured media storage

## Module dependencies

In your host application module:

```csharp
[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAIHttpApiModule),
    typeof(SufiAIBlazorModule),
    typeof(SufiAIEntityFrameworkCoreModule) // or SufiAIMongoDbModule
)]
public class YourHostModule : AbpModule
{
}
```

Reference projects from `modules/ai/` during platform development, or NuGet packages `SufiChain.SufiPlatform.SufiAI.*` in consuming solutions.

**Blazor WebAssembly:** also add `SufiAIBlazorWebAssemblyModule` on the client and `SufiAIHttpApiClientModule` for proxies.

## Database

Add AI Management to your host `DbContext` (EF) or MongoDB context configuration per module integration docs, then create and apply migrations from your host (user-run):

```bash
dotnet ef migrations add AddAI --project <YourEntityFrameworkCoreProject>
dotnet ef database update --project <YourEntityFrameworkCoreProject>
```

MongoDB: collections are created on use; no EF migrations.

## Configuration

No required `appsettings.json` section for API keys. Optionally:

```csharp
Configure<AIOptions>(options =>
{
    options.SeedFileStructure = true;
});
```

See [Configuration](configuration.md).

## Permissions

In **Permission Management**, grant roles as needed—for example:

- Admin: all `AI.*`
- Operator: workspaces, model config, test chat, RAG, usage view

See [Permissions](permissions.md).

## Verify installation

1. Run the host application.
2. Open `/admin/ai/workspaces`.
3. Create a workspace: name, API key, model → **Test connection** → **Save**.
4. Open `/admin/ai/test-chat`, select the workspace, send a message (try **Use streaming**).
5. Optional: add model configurations, configure RAG on workspace, index a document source, test `/admin/ai/rag`.

## File-Manager

If File-Manager is installed, enable `SeedFileStructure` so the AI file structure is seeded. Uploaded test media and integrations use that structure automatically.

## HelpDesk suite

When installing HelpDesk-style product modules, add the same AI Management dependencies to the product host and create a shared workspace, such as `helpdesk-default`, before indexing knowledge base content.
