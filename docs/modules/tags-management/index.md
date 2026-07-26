# Tags Management Module

Tag definitions and tag-to-entity link management with admin Blazor pages.

## Code location

`sufi-platform/modules/tags/`

## Packages

Package segment: **`Tags`** (`SufiChain.SufiPlatform.Tags.*`).

| Layer | Notes |
|-------|-------|
| Domain.Shared / Domain | Tags, tag links, permissions |
| Application[.Contracts] | Tag and tag-link app services |
| HttpApi[.Client] | HTTP surface |
| Blazor / Blazor.Server | Admin UI only (no Public layer) |
| EntityFrameworkCore / MongoDB | Dual persistence |

## Capabilities

- Tag CRUD
- Tag link assign / unassign
- Policy provider for permission-gated tag operations

## Permissions

- `TagsManagement.Tags` — Create, Update, Delete
- `TagsManagement.TagLinks` — Assign, Unassign

Confirm exact names in Domain.Shared if they differ.

## Related

- [Package Map](../../reference/package-map.md)
- [Module Catalog](../index.md)
