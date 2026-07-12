# Users Module

> **KB:** See workspace Obsidian vault `.obsidian/Sufi Platform/Modules/Users.md` for verified capabilities.

## Code location

`sufi-platform/modules/users/`

## Quick facts

- Lightweight user lookup — not full user admin (see Identity module)
- `IUserLookupAppService` and public selector components
- Blazor.Public only (no admin Blazor or HttpApi)
- EF Core + MongoDB

## Start in source

- `SufiChain.SufiPlatform.Users.Application` — `UserLookupAppService`
- `SufiChain.SufiPlatform.Users.Blazor.Public` — `SufiUserSelector`, `SufiUserSelect`
