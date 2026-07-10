# Framework Overview

The public framework source lives under `src/framework/`. This is the part of the repository you open when you need to understand shared UI composition, host integration, authentication plumbing, storage support, or the CLI used to scaffold and maintain products.

For contributors, the framework matters because it defines the reusable rules that every module and host should follow. When a behavior belongs to the platform rather than to one business module, it usually starts here.

## Framework projects

| Project | Why it matters |
| --- | --- |
| `SufiChain.SufiAbp.UI.Abstractions` | Contracts for themes, menus, toolbars, alerts, notifications, branding, user context, and browser-facing behaviors |
| `SufiChain.SufiAbp.UI.Domain.Shared` | Shared UI resources and domain-shared types used across framework and modules |
| `SufiChain.SufiAbp.UI.Services` | Default implementations behind menu, toolbar, theme, and alert services |
| `SufiChain.SufiAbp.UI.Blazor` | Core Blazor composition, `SufiAbpComponentBase`, and shared platform UI behavior |
| `SufiChain.SufiAbp.UI.Blazor.Server` | Server-hosted Blazor integration |
| `SufiChain.SufiAbp.UI.Blazor.WebAssembly` | WebAssembly-hosted Blazor integration |
| `SufiChain.SufiAbp.UI.Abp` | Bridges from SufiAbp abstractions into ABP runtime services |
| `SufiChain.SufiAbp.AspNetCore` | Shared ASP.NET Core platform infrastructure |
| `SufiChain.SufiAbp.AspNetCore.Authentication.Abstractions` | Shared authentication contracts and options |
| `SufiChain.SufiAbp.AspNetCore.Authentication.Server` | Server-side authentication integration |
| `SufiChain.SufiAbp.AspNetCore.Authentication.WebAssembly` | WebAssembly authentication integration |
| `SufiChain.SufiAbp.AspNetCore.Authentication.OpenIdConnect` | OpenID Connect integration |
| `SufiChain.SufiAbp.AspNetCore.Authentication.OAuth` | OAuth-oriented integration |
| `SufiChain.SufiAbp.Data` | Shared data helpers used across the platform |
| `SufiChain.SufiAbp.BlobStoring.S3Provider` | S3-compatible blob storage support |
| `SufiChain.SufiAbp.CLI.Core` | Shared CLI logic and scaffolding services |
| `SufiChain.SufiAbp.CLI` | The `sufi` command-line entry point |

## Where developers usually start

Open these areas first depending on the problem you are working on:

- `SufiChain.SufiAbp.UI.Blazor` when you need to understand shared component behavior or base classes
- `SufiChain.SufiAbp.UI.Abstractions` when you need a contract or extension point
- `SufiChain.SufiAbp.UI.Services` when you need the default implementation behind a UI abstraction
- `SufiChain.SufiAbp.UI.Abp` when platform UI behavior depends on ABP runtime services
- `SufiChain.SufiAbp.CLI.Core` when a change affects scaffolding or generation workflow

## Core concepts

### `SufiAbpComponentBase`

`SufiAbpComponentBase` is the standard base type for platform Blazor components. It centralizes common behaviors such as:

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
