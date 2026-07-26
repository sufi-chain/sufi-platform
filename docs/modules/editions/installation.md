# Editions Installation

Add the Editions packages that match your host shape (Application, HttpApi, Blazor, and either EntityFrameworkCore or MongoDB).

Example package family:

```text
SufiChain.SufiPlatform.Editions.Domain.Shared
SufiChain.SufiPlatform.Editions.Domain
SufiChain.SufiPlatform.Editions.Application.Contracts
SufiChain.SufiPlatform.Editions.Application
SufiChain.SufiPlatform.Editions.HttpApi
SufiChain.SufiPlatform.Editions.Blazor
SufiChain.SufiPlatform.Editions.EntityFrameworkCore   # or .MongoDB
```

Register the corresponding `Sufi*Editions*Module` types in the host module `DependsOn` list, then add EF/Mongo DbContext configuration the same way as other dual-persistence modules.

Editions is not yet scaffolded by `sufi new` by default.

## Related

- [Overview](overview.md)
- [Permissions](permissions.md)
- [Creating modules](../creating-modules.md)
