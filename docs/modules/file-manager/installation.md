# File Manager Installation

## Main packages

- `SufiChain.SufiPlatform.FileManager.Application.Contracts`
- `SufiChain.SufiPlatform.FileManager.Application`
- `SufiChain.SufiPlatform.FileManager.Domain.Shared`
- `SufiChain.SufiPlatform.FileManager.Domain`
- `SufiChain.SufiPlatform.FileManager.HttpApi`
- `SufiChain.SufiPlatform.FileManager.HttpApi.Client`
- `SufiChain.SufiPlatform.FileManager.Blazor`
- `SufiChain.SufiPlatform.FileManager.Blazor.Public`
- `SufiChain.SufiPlatform.FileManager.Blazor.Server`
- `SufiChain.SufiPlatform.FileManager.Blazor.WebAssembly`
- `SufiChain.SufiPlatform.FileManager.EntityFrameworkCore`
- `SufiChain.SufiPlatform.FileManager.MongoDB`

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
