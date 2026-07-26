# Module Catalog

Canonical catalog of **19** first-party modules. Source lives under `modules/` with short folder names; this docs tree often uses longer display folder names. Package segments are listed in [Package Map](../reference/package-map.md).

Each module folder should explain what responsibility it owns, how it relates to neighbors, and where to start in source.

## Administration and platform modules

| Module | Source folder | Responsibility | Canonical docs |
| --- | --- | --- | --- |
| Account | `account` | Sign-in, registration, profile, password, 2FA/OTP | [Account](account/index.md) |
| Audit Logging | `audit-logging` | Requests, actions, exceptions, entity changes | [Audit Logging](audit-logging/index.md) |
| Background Jobs | `jobs` | Job store, admin monitoring, retry | [Background Jobs](background-jobs/index.md) |
| Editions | `editions` | Named editions bound to feature policy values | [Editions](editions/index.md) |
| Feature Management | `features` | Feature definitions and values | [Feature Management](feature-management/index.md) |
| Identity | `identity` | Users, roles, OUs, security logs, dynamic claims | [Identity](identity/index.md) |
| Localization Management | `localization` | Localization resources and texts | [Localization Management](localization-management/index.md) |
| Permission Management | `permissions` | Permission grants and authorization store | [Permission Management](permission-management/index.md) |
| Setting Management | `settings` | Settings administration and groups | [Setting Management](setting-management/index.md) |
| Tenant Management | `tenants` | Host-level tenant administration | [Tenant Management](tenant-management/index.md) |

## Business and experience modules

| Module | Source folder | Responsibility | Canonical docs |
| --- | --- | --- | --- |
| AI | `ai` | Workspaces, RAG, MCP, usage analytics | [AI](ai/index.md) |
| Calendar | `calendar` | Events, availability, MCP scheduling tools | [Calendar](calendar/index.md) |
| File Manager | `file-manager` | Files, media, uploads, folders | [File Manager](file-manager/index.md) |
| Menu Management | `menus` | Dynamic menus and public menu API | [Menu Management](menu-management/index.md) |
| Short Link Generator | `short-links` | Short URLs, redirects, click analytics | [Short Link Generator](short-link-generator/index.md) |
| Tags Management | `tags` | Tags and tag-to-entity links | [Tags Management](tags-management/index.md) |
| Users | `users` | User lookup and public selectors | [Users](users/index.md) |

## Infrastructure modules

| Module | Source folder | Responsibility | Canonical docs |
| --- | --- | --- | --- |
| Blob Storing Database | `blob-database` | Database-backed blob provider (EF + MongoDB) | [Blob Storing Database](blob-storing-database/index.md) |
| OpenIddict | `openiddict` | OAuth/OIDC server integration for tiered hosts | [OpenIddict](openiddict/index.md) |

## Standard module page set

Each module should provide the same page set so readers can move through the catalog without guessing where to find information:

- `index.md`
- `overview.md`
- `features.md`
- `installation.md`
- `configuration.md`
- `usage.md`
- `architecture.md`
- `permissions.md`
- `settings.md`
- `extending.md`
- `api.md`
