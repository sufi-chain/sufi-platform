# Identity Architecture

The Identity module wraps and extends ABP identity capabilities.

## Projects

- `Application.Contracts`
- `Application`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- `Domain.Shared`
- `Public.Blazor`

## Important implementation notes

- the permission group name is `AbpIdentity`, so the module integrates directly into ABP's identity permission model
- Application references `Volo.Abp.Identity.Domain`
- Blazor references permission-management contracts for integrated permission workflows
