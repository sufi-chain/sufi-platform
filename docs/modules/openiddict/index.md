# OpenIddict Module

OAuth 2.0 / OpenID Connect server integration. Domain and AspNetCore layers only — no Blazor admin UI or Application/HttpApi in this module.

## Code location

`sufi-platform/modules/openiddict/`

## Packages

| Layer | Project |
|-------|---------|
| Domain.Shared | `SufiChain.SufiPlatform.OpenIddict.Domain.Shared` |
| Domain | `SufiChain.SufiPlatform.OpenIddict.Domain` |
| AspNetCore | `SufiChain.SufiPlatform.OpenIddict.AspNetCore` |
| EntityFrameworkCore | `SufiChain.SufiPlatform.OpenIddict.EntityFrameworkCore` |
| MongoDB | `SufiChain.SufiPlatform.OpenIddict.MongoDB` |
| Permission bridge | `SufiChain.SufiPlatform.Permissions.Domain.OpenIddict` |

## Capabilities

- OpenIddict application, authorization, scope, and token entities + repositories (EF + MongoDB)
- OpenIddict Core store adapters (`OpenIddictApplicationStore`, authorization/scope/token stores) wired in the domain module
- ASP.NET Core host integration for AuthServer tiered solutions (`--tiered`)
- Permission management domain bridge for client applications

## Deferred vs ABP

Managers, definition caches, Mapperly mappers, and token cleanup workers from upstream ABP are intentionally deferred after the store adapters.

## Related

- [Authentication](../../framework/authentication.md)
- [Identity](../identity/index.md)
- [Account](../account/index.md)
- [Security](../../operations/security.md)
