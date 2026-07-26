# Tenant Management Installation

## Main packages

- `SufiChain.SufiPlatform.Tenants.Application.Contracts`
- `SufiChain.SufiPlatform.Tenants.Application`
- `SufiChain.SufiPlatform.Tenants.Domain.Shared`
- `SufiChain.SufiPlatform.Tenants.Domain`
- `SufiChain.SufiPlatform.Tenants.HttpApi`
- `SufiChain.SufiPlatform.Tenants.HttpApi.Client`
- `SufiChain.SufiPlatform.Tenants.Blazor`

## Important dependency pattern

The Blazor package references:

- `SufiChain.SufiPlatform.Features.Blazor`
- `SufiChain.SufiPlatform.Settings.Blazor`

This indicates that tenant administration is a central place where related administrative capabilities are composed.
