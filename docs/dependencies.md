# Dependencies and Libraries

This page is the main reference for libraries and packages used by the public Sufi Platform source under `/src`.

- **Version pinning:** `versions.props`
- **Package references:** project `.csproj` files and shared `common.props` where applicable

## Sufi Platform packages in the public source

| Package or group | Role |
| --- | --- |
| `SufiChain.SufiAbp.UI.Abstractions` | UI contracts for theming, menus, toolbars, notifications, branding, and user context |
| `SufiChain.SufiAbp.UI.Domain.Shared` | Shared UI-domain types and resources |
| `SufiChain.SufiAbp.UI.Services` | Default UI service implementations |
| `SufiChain.SufiAbp.UI.Blazor` | Shared Blazor platform layer including `SufiAbpComponentBase` |
| `SufiChain.SufiAbp.UI.Blazor.Server` | Server-hosted Blazor support |
| `SufiChain.SufiAbp.UI.Blazor.WebAssembly` | WebAssembly-hosted Blazor support |
| `SufiChain.SufiAbp.UI.Abp` | ABP adapter layer |
| `SufiChain.SufiAbp.AspNetCore` | Shared ASP.NET Core platform infrastructure |
| `SufiChain.SufiAbp.AspNetCore.Authentication.*` | Authentication packages for server, WASM, OAuth, and OIDC scenarios |
| `SufiChain.SufiAbp.Data` | Shared data-layer support utilities |
| `SufiChain.SufiAbp.BlobStoring.S3Provider` | S3-compatible blob storage support |
| `SufiChain.SufiAbp.CLI.Core` / `SufiChain.SufiAbp.CLI` | CLI logic and executable |
| `SufiChain.SufiAbp.*` modules under `src/modules` | First-party business and administration modules |

## Independent products in the platform story

SufiBlazor and KomTheme are part of the broader Sufi Platform offering, but they are developed and versioned independently.

In this docs set they are covered from a platform-product perspective, while their detailed technical package documentation should remain in their own repositories.

## Key third-party libraries

| Area | Libraries | Notes |
| --- | --- | --- |
| Framework | ABP Framework (`Volo.Abp.*`) | Core application, modularity, permissions, tenancy, and administrative foundations |
| Runtime | ASP.NET Core 10 and Blazor | Main runtime stack |
| Localization | `Microsoft.Extensions.Localization` | Localization infrastructure across framework and modules |
| Data | Entity Framework Core 10 | Used by modules and hosts that choose EF Core |
| Logging | Serilog packages | Structured logging support |
| Testing | xUnit, NSubstitute, Shouldly, bUnit, Coverlet | Unit, integration, and UI test tooling |
| Build | Fody / ConfigureAwait.Fody | Build-time weaving support |
| Media and processing | ImageSharp, FFMpegCore | Used by modules such as file manager |

## Notes

- Exact versions are defined in `versions.props`.
- Module-specific third-party dependencies should be documented again in module docs where they materially affect installation or behavior.
