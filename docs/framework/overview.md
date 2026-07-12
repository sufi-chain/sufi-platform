# Framework Overview

The public framework source lives under `src/framework/`. This is the part of the repository you open when you need to understand shared UI composition, host integration, authentication plumbing, storage support, or the CLI used to scaffold and maintain products.

For contributors, the framework matters because it defines the reusable rules that every module and host should follow. When a behavior belongs to the platform rather than to one business module, it usually starts here.

## Framework projects

| Project | Why it matters |
| --- | --- |
| `SufiChain.SufiPlatform.UI.Abstractions` | Contracts for themes, menus, toolbars, alerts, notifications, branding, user context, and browser-facing behaviors |
| `SufiChain.SufiPlatform.UI.Domain.Shared` | Shared UI resources and domain-shared types used across framework and modules |
| `SufiChain.SufiPlatform.UI.Services` | Default implementations behind menu, toolbar, theme, and alert services |
| `SufiChain.SufiPlatform.UI.Blazor` | Core Blazor composition, `SufiComponentBase`, and shared platform UI behavior |
| `SufiChain.SufiPlatform.UI.Blazor.Server` | Server-hosted Blazor integration |
| `SufiChain.SufiPlatform.UI.Blazor.WebAssembly` | WebAssembly-hosted Blazor integration |
| `SufiChain.SufiPlatform.UI.Abp` | Bridges from Sufi Platform abstractions into ABP runtime services |
| `SufiChain.SufiPlatform.AspNetCore` | Shared ASP.NET Core platform infrastructure |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.Abstractions` | Shared authentication contracts and options |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.Server` | Server-side authentication integration |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.WebAssembly` | WebAssembly authentication integration |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.OpenIdConnect` | OpenID Connect integration |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.OAuth` | OAuth-oriented integration |
| `SufiChain.SufiPlatform.Data` | Shared data helpers used across the platform |
| `SufiChain.SufiPlatform.BlobStoring.S3Provider` | S3-compatible blob storage support |
| `SufiChain.SufiPlatform.CLI.Core` | Shared CLI logic and scaffolding services |
| `SufiChain.SufiPlatform.CLI` | The `sufi` command-line entry point |

## Where developers usually start

Open these areas first depending on the problem you are working on:

- `SufiChain.SufiPlatform.UI.Blazor` when you need to understand shared component behavior or base classes
- `SufiChain.SufiPlatform.UI.Abstractions` when you need a contract or extension point
- `SufiChain.SufiPlatform.UI.Services` when you need the default implementation behind a UI abstraction
- `SufiChain.SufiPlatform.UI.Abp` when platform UI behavior depends on ABP runtime services
- `SufiChain.SufiPlatform.CLI.Core` when a change affects scaffolding or generation workflow

## Core concepts

### `SufiComponentBase`

`SufiComponentBase` is the standard base type for platform Blazor components. It centralizes common behaviors such as:

- localization through `L["Key"]`
- user and tenant context access
- UI communication and notifications
- loading helpers such as `ExecuteWithLoadingAsync`
- lazy access to common platform services

### Contributor-based composition

The framework relies on contributors so modules can extend shared UI surfaces without hard-wiring themselves into a single host.

Common extension points include:

- `IMenuContributor`
- `IToolbarContributor`
- theme registration
- layout composition hooks
- settings group contributors in relevant modules

## Dependency direction

At a high level:

- abstractions live in `UI.Abstractions`
- default behavior lives in `UI.Services`
- shared Blazor behavior lives in `UI.Blazor`
- host-specific Blazor integration lives in the server and WebAssembly packages
- ABP bridges live in `UI.Abp`
- infrastructure concerns such as authentication, storage, and CLI remain adjacent to the UI stack rather than inside business modules

## Related docs

- [ABP Integration](abp-integration.md)
- [Authentication](authentication.md)
- [Module Architecture](module-architecture.md)
- [Developer Conventions](developer-conventions.md)
