# Tenant Management Installation

## Main packages

- `SufiChain.SufiPlatform.TenantManagement.Application.Contracts`
- `SufiChain.SufiPlatform.TenantManagement.Application`
- `SufiChain.SufiPlatform.TenantManagement.Domain.Shared`
- `SufiChain.SufiPlatform.TenantManagement.Domain`
- `SufiChain.SufiPlatform.TenantManagement.HttpApi`
- `SufiChain.SufiPlatform.TenantManagement.HttpApi.Client`
- `SufiChain.SufiPlatform.TenantManagement.Blazor`

## Important dependency pattern

The Blazor package references:

- `SufiChain.SufiPlatform.FeatureManagement.Blazor`
- `SufiChain.SufiPlatform.SettingManagement.Blazor`

This indicates that tenant administration is a central place where related administrative capabilities are composed.
