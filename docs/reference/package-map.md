# Package Map

Canonical map of public Sufi Platform packages. Exact versions live in `sufi-platform/versions.props`.

## Naming

| Term | Meaning |
|------|---------|
| **Sufi Platform** | Product and platform name |
| **`SufiChain.SufiPlatform.*`** | NuGet / project package family |
| **`Sufi*` type prefix** | Code prefix for framework and module types (`SufiModule`, `SufiComponentBase`, …) |
| **ABP Framework** | Upstream foundation consumed as `Volo.Abp.*` |

## Framework packages (31)

Source: `framework/`. After Framework Reduction, infrastructure such as EF providers, MongoDB, EventBus, and Caching is consumed directly from `Volo.Abp.*`.

### Core / DDD (3)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.Core` | `SufiModule`, host extensions |
| `SufiChain.SufiPlatform.Ddd.Application.Contracts` | DTO bases, `ISufiCrudAppService` |
| `SufiChain.SufiPlatform.Ddd.Application` | `SufiApplicationService`, CRUD services |

### UI (6)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.UI.Abstractions` | Menu, toolbar, layout, theme contracts |
| `SufiChain.SufiPlatform.UI.Domain.Shared` | Framework localization resources |
| `SufiChain.SufiPlatform.UI.Services` | Default menu/theme/alert/tenant services |
| `SufiChain.SufiPlatform.UI.Blazor` | `SufiComponentBase`, page chrome |
| `SufiChain.SufiPlatform.UI.Blazor.Server` | Blazor Server integrations |
| `SufiChain.SufiPlatform.UI.Blazor.WebAssembly` | Blazor WASM integrations |

### SufiAI (2)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.SufiAI.Abstractions` | Chat client / kernel contracts |
| `SufiChain.SufiPlatform.SufiAI` | Workspace options, keyed `SufiAI.ChatClient_` / `SufiAI.Kernel_` services |

### SufiCom (2)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.SufiCom.Abstractions` | Channel, email, SMS, voice contracts |
| `SufiChain.SufiPlatform.SufiCom` | Senders, jobs, notifications |

### AspNetCore / Authentication (7)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.AspNetCore` | ASP.NET Core integration |
| `SufiChain.SufiPlatform.AspNetCore.Mvc` | `SufiControllerBase` |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.Abstractions` | Auth hosting abstractions |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.OpenIdConnect` | OIDC |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.Server` | Server-side auth |
| `SufiChain.SufiPlatform.AspNetCore.Authentication.WebAssembly` | WASM OIDC |
| `SufiChain.SufiPlatform.Authorization` | Authorization helpers |

### Captcha (3)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.Captcha` | Abstractions + math captcha |
| `SufiChain.SufiPlatform.Captcha.Recaptcha` | Google reCAPTCHA v2 |
| `SufiChain.SufiPlatform.Captcha.Turnstile` | Cloudflare Turnstile |

### Blob / Data / Features / Validation / Templating (6)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.BlobStoring.S3Provider` | S3 blob provider |
| `SufiChain.SufiPlatform.Data` | Seed / data helpers |
| `SufiChain.SufiPlatform.Features` | Feature helpers |
| `SufiChain.SufiPlatform.Validation` | Branded validation localization |
| `SufiChain.SufiPlatform.TextTemplating` | Text templating |
| `SufiChain.SufiPlatform.TextTemplating.Scriban` | Scriban engine |

### CLI (2)

| Package | Role |
|---------|------|
| `SufiChain.SufiPlatform.CLI.Core` | Template pipeline, module registry |
| `SufiChain.SufiPlatform.CLI` | `sufi` global tool |

## First-party modules (19)

Source folders under `modules/` use short names. Package segments follow the table.

| Docs folder | Source folder | Package segment |
|-------------|---------------|-----------------|
| `account` | `account` | `Account` |
| `ai` | `ai` | `SufiAI` (module projects) |
| `audit-logging` | `audit-logging` | `AuditLogging` |
| `background-jobs` | `jobs` | `BackgroundJobs` |
| `blob-storing-database` | `blob-database` | `BlobDatabase` |
| `calendar` | `calendar` | `Calendar` |
| `editions` | `editions` | `Editions` |
| `feature-management` | `features` | `Features` |
| `file-manager` | `file-manager` | `FileManager` |
| `identity` | `identity` | `Identity` |
| `localization-management` | `localization` | `Localization` |
| `menu-management` | `menus` | `Menus` |
| `openiddict` | `openiddict` | `OpenIddict` |
| `permission-management` | `permissions` | `Permissions` |
| `setting-management` | `settings` | `Settings` |
| `short-link-generator` | `short-links` | `ShortLinks` |
| `tags-management` | `tags` | `Tags` |
| `tenant-management` | `tenants` | `Tenants` |
| `users` | `users` | `Users` |

## Independent products

SufiBlazor and SufiTheme are versioned separately. Capability-level links may appear here; component and layout docs stay in:

- `independent-projects/sufi-blazor/docs/`
- `independent-projects/sufi-theme/docs/`

## Related

- [Technology stack](technology-stack.md)
- [Framework Overview](../framework/overview.md)
- [Module Catalog](../modules/index.md)
- [Architecture decisions](../architecture/decisions.md)
