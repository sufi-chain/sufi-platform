# Users Module

> **KB:** See workspace Obsidian vault `.obsidian/SufiAbp/Modules/Users.md` for verified capabilities.

## Code location

`sufi-abp/modules/users/`

## Quick facts

- Lightweight user lookup — not full user admin (see Identity module)
- `IUserLookupAppService` and public selector components
- Blazor.Public only (no admin Blazor or HttpApi)
- EF Core + MongoDB

## Start in source

- `SufiChain.SufiAbp.Users.Application` — `UserLookupAppService`
- `SufiChain.SufiAbp.Users.Blazor.Public` — `SufiUserSelector`, `SufiUserSelect`
