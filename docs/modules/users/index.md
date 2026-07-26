# Users Module

Lightweight user **lookup** module — not a full user admin replacement. Prefer [Identity](../identity/index.md) for administration.

## Code location

`sufi-platform/modules/users/`

## Packages

Package segment: **`Users`** (`SufiChain.SufiPlatform.Users.*`).

| Layer | Notes |
|-------|-------|
| Domain.Shared / Domain | Lookup domain |
| Application[.Contracts] | `IUserLookupAppService` |
| Blazor.Public | Public selector components |
| EntityFrameworkCore / MongoDB | Persistence |
| — | No HttpApi or admin Blazor layers |

## Capabilities

- `IUserLookupAppService` / `UserLookupAppService`
- Public components: `SufiUserSelector`, `SufiUserSelect`, `SufiUserSelectorUserGrid`
- Inline lookup base for embedding pickers in other modules

## Permissions

- `SufiUsers.UserLookup`

## Related

- [Identity](../identity/index.md)
- [Package Map](../../reference/package-map.md)
