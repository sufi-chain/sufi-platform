# File Manager Installation

## Main packages

- `SufiChain.SufiAbp.FileManager.Application.Contracts`
- `SufiChain.SufiAbp.FileManager.Application`
- `SufiChain.SufiAbp.FileManager.Domain.Shared`
- `SufiChain.SufiAbp.FileManager.Domain`
- `SufiChain.SufiAbp.FileManager.HttpApi`
- `SufiChain.SufiAbp.FileManager.HttpApi.Client`
- `SufiChain.SufiAbp.FileManager.Blazor`
- `SufiChain.SufiAbp.FileManager.Blazor.Public`
- `SufiChain.SufiAbp.FileManager.Blazor.Server`
- `SufiChain.SufiAbp.FileManager.Blazor.WebAssembly`
- `SufiChain.SufiAbp.FileManager.EntityFrameworkCore`
- `SufiChain.SufiAbp.FileManager.MongoDB`

## Typical host setup

For an EF Core host, the module usually includes:

- application layer
- EF Core persistence layer
- HTTP API layer
- Blazor UI and HTTP API client for interactive hosts

For MongoDB hosts, replace the EF Core package with the MongoDB package.

## Related guidance

Use the existing detailed technical pages for deeper setup instructions:

- [Integration Guide](integration-guide.md)
- [Configuration](configuration.md)
- [Blazor Components Guide](blazor-components-guide.md)
