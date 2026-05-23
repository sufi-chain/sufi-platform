# Module Catalog

This section is the canonical catalog of first-party modules under `src/modules`. Use it when you need to decide whether a requirement is already covered by the platform, find the right module to extend, or identify which module docs to read before changing a host application.

Each module folder in this section should explain three things clearly: what responsibility the module owns, how it relates to neighboring modules, and where a developer should start in the source when contributing.

## Administration and platform modules

| Module | Responsibility | Canonical docs |
| --- | --- | --- |
| Account | End-user account flows such as sign-in, registration, profile, and password management | [Account](account/index.md) |
| Audit Logging | Operational visibility into requests, actions, exceptions, and entity changes | [Audit Logging](audit-logging/index.md) |
| Background Jobs | Administrative monitoring and control of background jobs | [Background Jobs](background-jobs/index.md) |
| Feature Management | Feature definition and feature value management across tenants and products | [Feature Management](feature-management/index.md) |
| Identity | User, role, organization-unit, and security-log administration | [Identity](identity/index.md) |
| Localization Management | Management of localization resources and texts | [Localization Management](localization-management/index.md) |
| Permission Management | Shared permission grant management and authorization support | [Permission Management](permission-management/index.md) |
| Setting Management | Centralized settings administration and settings-group composition | [Setting Management](setting-management/index.md) |
| Tenant Management | Host-level tenant administration and tenant-aware operations | [Tenant Management](tenant-management/index.md) |

## Business and experience modules

| Module | Responsibility | Canonical docs |
| --- | --- | --- |
| File Manager | Shared file, media, upload, folder, and asset-management workflows | [File Manager](file-manager/index.md) |
| Short Link Generator | Short URL creation, redirect handling, and click analytics | [Short Link Generator](short-link-generator/index.md) |

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
