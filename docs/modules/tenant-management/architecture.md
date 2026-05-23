# Tenant Management Architecture

This module wraps ABP tenant management and extends it through a platform-aligned administrative UI.

## Projects

- `Application.Contracts`
- `Application`
- `Domain.Shared`
- `Domain`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`

## Important notes

- `Domain.Shared` depends on `Volo.Abp.Features`, which reflects tenant/feature integration
- the Blazor project composes feature-management and setting-management modules
- the module includes a tenant-selector area for context switching support
