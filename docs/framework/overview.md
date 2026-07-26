# Framework Overview

The public framework source lives under `framework/`. Open it when you need shared UI composition, host integration, authentication, storage helpers, SufiAI/SufiCom, captcha, or the `sufi` CLI.

After Framework Reduction, EF Core providers, MongoDB, EventBus, Caching, and similar infrastructure are consumed as **`Volo.Abp.*`**. The **31** packages below are value-add only. Full inventory: [Package Map](../reference/package-map.md).

## Package families

| Family | Packages | Why it matters |
| --- | --- | --- |
| Core / DDD | Core, Ddd.Application.Contracts, Ddd.Application | `SufiModule`, `SufiApplicationService`, CRUD contracts |
| UI | UI.Abstractions, UI.Domain.Shared, UI.Services, UI.Blazor, UI.Blazor.Server, UI.Blazor.WebAssembly | Contracts, `SufiComponentBase`, default UI services |
| SufiAI | SufiAI.Abstractions, SufiAI | Cross-module chat/kernel workspace options |
| SufiCom | SufiCom.Abstractions, SufiCom | Email, SMS, voice, channel senders |
| AspNetCore / Auth | AspNetCore, Mvc, Authentication.*, Authorization | `SufiControllerBase`, OIDC, server/WASM auth |
| Captcha | Captcha, Captcha.Recaptcha, Captcha.Turnstile | Bot protection |
| Data / Blob / Features / Validation / Templating | Data, BlobStoring.S3Provider, Features, Validation, TextTemplating[.Scriban] | Seeds, S3, features, validation, templates |
| CLI | CLI.Core, CLI | `sufi` tool and template pipeline |

## Where developers usually start

- `UI.Blazor` — shared component behavior and `SufiComponentBase`
- `UI.Abstractions` — contracts and extension points
- `UI.Services` — default implementations behind those contracts
- `SufiAI` — keyed chat/kernel registration for modules
- `CLI.Core` — scaffolding, module registry, template markers

## Core concepts

### `SufiModule`

Bridges ABP module lifecycle (`PreConfigureServices`, `ConfigureServices`, `OnApplicationInitialization`) into Sufi-namespaced modules.

### `SufiComponentBase`

Standard Blazor base for platform components: localization (`L["Key"]`), user/tenant context, notifications, loading helpers, and lazy access to common services.

### Contributor-based composition

Modules extend shared UI without hard-wiring hosts:

- `IMenuContributor`
- `IToolbarContributor`
- theme registration and layout hooks
- settings-group contributors in relevant modules

## Dependency direction

- Abstractions contracts in `UI.Abstractions`
- Defaults in `UI.Services`
- Shared Blazor in `UI.Blazor`
- Host-specific Blazor in Server / WebAssembly packages
- Infrastructure (EF, Mongo, EventBus, Caching) via `Volo.Abp.*`, not Sufi wrappers

## Related docs

- [Package Map](../reference/package-map.md)
- [ABP Integration](abp-integration.md)
- [Authentication](authentication.md)
- [Module Architecture](module-architecture.md)
- [Developer Conventions](developer-conventions.md)
- [Architecture decisions](../architecture/decisions.md)
- [CLI](cli.md)
- [Communication overview](communication-overview.md)
